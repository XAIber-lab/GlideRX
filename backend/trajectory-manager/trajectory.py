import numpy as np
import math
from scipy.spatial.transform import Rotation as Rot


def process_trajectory(complete_trajectory, traj_descr, traj_file_name):
    for sect in traj_descr:
        complete_trajectory.concatenateTrajectorySection(sect)
    complete_trajectory.generateConcatenatedTrajectory()
    write_trajectory_to_file(complete_trajectory, traj_file_name)


def write_trajectory_to_file(complete, traj_file_name):
    with open(traj_file_name, "w") as traj_file:
        csv_string = "X , Y , Z , Q0 , Q1 , Q2 , Q3\n"
        traj_file.writelines(csv_string)
        for i in range(len(complete.trajectory)):
            csv_string = str(complete.trajectory[i][0]) + " , "
            csv_string += str(complete.trajectory[i][1]) + " , "
            csv_string += str(complete.trajectory[i][2]) + " , "
            csv_string += str(complete.quaternions[i][0]) + " , "
            csv_string += str(complete.quaternions[i][1]) + " , "
            csv_string += str(complete.quaternions[i][2]) + " , "
            csv_string += str(complete.quaternions[i][3]) + "\n"
            traj_file.writelines(csv_string)


class SectionMetadata:
    def __init__(self, forward_speed, vertical_speed, radius, duration) -> None:
        self.forward_speed = forward_speed
        self.vertical_speed = vertical_speed
        self.radius = radius
        self.duration = duration
        self.getBankAngle()

    def getBankAngle(self):
        # Now implement some rule to get the BankAngle from the params. by now only radius is used (to be improved ???)
        radius_to_bankAngle = 0.7e-2  # proportionality between trajectory radius and bank angle
        max_bankAngle = math.radians(90.0)
        min_radius = 30.0
        r = np.abs(self.radius)
        bankAngle = 0
        if r == 0:  # straight
            bankAngle = 0
        else:
            if r < min_radius:
                bankAngle = max_bankAngle
            else:
                bankAngle = max_bankAngle * np.exp(-radius_to_bankAngle * (r - min_radius))
        if self.radius > 0:  # assign the sign of bankAngle consistent with turn direction
            bankAngle = -bankAngle
        self.bankAngle = bankAngle

class TrajectorySection:
    def __init__(self, forward_speed = 27.78, vertical_speed = 0.0, radius = 0.0, duration = 10.0) -> None:
        # if positive radius, left turn, right if negative
        n_points = math.floor(duration)
        if n_points <= 1:
            raise ValueError("The duration of the section must be greater than 1 seconds")

        # Time points
        t = np.linspace(0, n_points, n_points)
        
        # Straight path case (radius = 0 used conventionally for straight trajectory)
        if radius == 0:
            # Calculate positions for straight trajectory
            x = forward_speed * t
            y = np.zeros_like(t)
            z = vertical_speed * t
        else:
            # Circular or helical path
            # Angular speed for circular motion (it's a signed quantity)
            angular_speed = forward_speed / radius

            # Calculate positions for circular/helical trajectory
            # r is just the module of the vector here (always positive)
            r = np.abs(radius)
            x = r * np.cos(angular_speed * t) - r
            y = r * np.sin(angular_speed * t)
            z = vertical_speed * t  # Vertical motion remains constant in both cases

        metadata = SectionMetadata(forward_speed, vertical_speed, radius, duration)
        self.trajectory = np.stack((x, y, z)).swapaxes(0, 1)
        self.metadata = metadata


def rotatePoints(points, cos_angle, sin_angle):
    # do not change anymore
    rotation_matrix = np.array([
        [ cos_angle,  sin_angle, 0],
        [-sin_angle,  cos_angle, 0],
        [0,           0,         1]
    ])
    rotated_points = np.dot(points, rotation_matrix)
    return rotated_points


class Trajectory:

    def __init__(self) -> None:
        self.trajectory_sections = []
        self.trajectory_metadata = []
        self.trajectory = []
        self.quaternions = []
        self.fwd_speed = []
        self.vert_speed = []
        self.traj_origin_x = 0
        self.traj_origin_y = 0
        self.traj_origin_z = 0

    def concatenateTrajectorySection(self, section: TrajectorySection):
        self.trajectory_sections.append(section.trajectory)
        self.trajectory_metadata.append(section.metadata)

    def getSinCos(self, a, b):
        na = a / np.sqrt(a.dot(a))
        nb = b / np.sqrt(b.dot(b))
        cos = na.dot(nb)
        c = np.cross(na, nb)
        nc = c / np.sqrt(c.dot(c))
        idx = np.argmax(np.abs(c))
        sgn = c[idx] / np.abs(c[idx])
        sin = sgn * np.abs(c[idx] / nc[idx])
        return sin, cos

    def computeBankAngleQuaternion(self, axis, angle):
        direction = np.array([1, 0, 0])  # axis / np.sqrt(axis.dot(axis))  # is this really necessary?
        return Rot.from_rotvec(angle * direction)

    def computeSegmentAlignementQuaternion(self, vector):
        orientation = vector / np.sqrt(vector.dot(vector))
        initial_vector = np.array([1, 0, 0])  # assume initial airplane orientation  along X axis
        rot_ax = np.cross(initial_vector, orientation)
        rotation_axis = rot_ax / np.sqrt(rot_ax.dot(rot_ax))
        sin, cos = self.getSinCos(initial_vector, orientation)
        angle = np.arccos(cos)
        return Rot.from_rotvec(angle * rotation_axis)

    def computeSectionQuaternions(self, section, metadata):
        bankAngle = metadata.bankAngle
        for i in range(len(section)-1):
            axis = section[i + 1] - section[i]
            alignment_rotation = self.computeSegmentAlignementQuaternion(axis)
            bank_angle_rotation = self.computeBankAngleQuaternion(axis, bankAngle)
            combined_rotation = alignment_rotation * bank_angle_rotation
            combined_rotation_quat = combined_rotation.as_quat()
            self.quaternions.append(combined_rotation_quat)
            if i == len(section)-2:
                self.quaternions.append(combined_rotation_quat)

    def computeSectionAlignementRotCoeff(self, v_section, v_trajectory):
        """
            Compute the angle needed to rotate the entire current section and align it to the trajectory
        """
        # compute the vector defined by the (X,Y) plane projection of the last  2 points of the trajectory
        # compute the vector defined by the (X,Y) plane projection of the first 2 points of the current  section
        # the cos of the angle comes from the  dot  product of the normalized vectors
        # the sin of the angle comes from the cross product of the normalized vectors
        v2d_trajectory = v_trajectory
        v2d_trajectory[-1] = 0
        v2d_section = v_section
        v2d_section[-1] = 0
        sin, cos = self.getSinCos(v2d_section, v2d_trajectory)
        return cos, sin

    def generateConcatenatedTrajectory(self):
        if len(self.trajectory_sections) < 1:
            raise RuntimeError("Provide trajectory sections using the method concatenateTrajectorySection and try again")
        for section_index in range(len(self.trajectory_sections)):
            curr_section = self.trajectory_sections[section_index]
            curr_metadata = self.trajectory_metadata[section_index]
            if len(curr_section) < 2:
                # for each section we need at least 2 points to compute the orientation
                # of the last segment of a section and of the first segment of the next
                raise RuntimeError("Provide a trajectory sections containing at least 2 points")
            if section_index == 0:
                self.trajectory = curr_section
                self.computeSectionQuaternions(curr_section, curr_metadata)
                for i in range(len(curr_section)):
                    self.fwd_speed.append(curr_metadata.forward_speed)
                    self.vert_speed.append(curr_metadata.vertical_speed)
                continue
            end_traj_orientation = self.trajectory[-1] - self.trajectory[-2]
            begin_sect_orientation = curr_section[1] - curr_section[0]
            cos_angle, sin_angle = self.computeSectionAlignementRotCoeff(begin_sect_orientation, end_traj_orientation)
            rotated_section = rotatePoints(curr_section, cos_angle, sin_angle)
            origin = self.trajectory[-1]
            # Remove the duplicated point at the beginning of the new section before concatenating
            self.trajectory = np.concatenate((self.trajectory, rotated_section[1:-1] + origin))
            self.computeSectionQuaternions(rotated_section[1:-1], curr_metadata)
            for i in range(len(rotated_section[1:-1])):
                self.fwd_speed.append(curr_metadata.forward_speed)
                self.vert_speed.append(curr_metadata.vertical_speed)
        
        # move the origin of the complete trajectory
        for i in range(len(self.trajectory)):
            self.trajectory[i, 0] += self.traj_origin_x
            self.trajectory[i, 1] += self.traj_origin_y
            self.trajectory[i, 2] += self.traj_origin_z

        # smooth out the orientation change by quaternion interpolation over the entire trajectory
        # interpolateTrajectoryQuaternions()
