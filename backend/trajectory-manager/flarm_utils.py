import numpy as np


class PFLAU_Info:
    TTC = None
    rel_vert_dist = None
    rel_hor_dist = None
    rel_bearing_angle = None
    ICAO_ID = None
    alarm_level = None


class PFLAA_Info:
    TTC = None
    relative_north = None
    relative_east = None
    rel_vert_dist = None
    ICAO_ID = None
    track = None
    alarm_level = None


def write_PFLAU_data_to_file(trajectories, PFLAU_file_name):
    """
    FLARM PFLAU string format:
    PFLAU,<RX>,<TX>,<GPS>,<Power>,<AlarmLevel>,<RelativeBearing>, <AlarmType>,<RelativeVertical>,<RelativeDistance>[,<ID>]
    :param trajectories: list of trajectories for the different airplanes in the scene
    :param PFLAU_file_name: name of the PFLAU file generated using airplane 1 as reference
    :return: nothing but writes the flarm PFLAU data
    """
    with open(PFLAU_file_name, "w") as PFLAU_file:
        csv_string = "PFLAU,RX,TX,GPS,Power,AlarmLevel,RelativeBearing,AlarmType,RelativeVertical,RelativeDistance,ID\n"
        ICAO_self = get_ICAO_ID(0)
        PFLAU_file.writelines(csv_string)
        fwd_speed_self = trajectories[0].fwd_speed
        vert_speed_self = trajectories[0].vert_speed
        # print("Traj vector len = ", len(trajectories[0].trajectory[:, 0]), "speed vector len = ", len(fwd_speed_self))
        generate_PFLAU_data(trajectories, PFLAU_file)


def write_PFLAA_data_to_file(trajectories, PFLAA_file_name):
    """
    FLARM PFLAA string format:
    PFLAA,<AlarmLevel>,<RelativeNorth>,<RelativeEast>, <RelativeVertical>,<IDType>,<ID>,
    <Track>,<TurnRate>,<GroundSpeed>,<ClimbRate>,<AcftType>[,<NoTrack>[,<Source>,<RSSI>]]
    NOTE: the optional parameters will be written anyways
    :param trajectories: list of trajectories for the different airplanes in the scene
    :param PFLAA_file_name: name of the PFLAA file generated using airplane 1 as reference
    :return: nothing but writes the flarm PFLAA data
    """
    with open(PFLAA_file_name, "w") as PFLAA_file:
        csv_string = "PFLAA,AlarmLevel,RelativeNorth,RelativeEast,RelativeVertical,IDType,ID,Track,TurnRate,GroundSpeed,ClimbRate,AcftType,NoTrack,Source,RSSI\n"
        ICAO_self = get_ICAO_ID(0)
        PFLAA_file.writelines(csv_string)
        fwd_speed_self = trajectories[0].fwd_speed
        vert_speed_self = trajectories[0].vert_speed
        # print("Traj vector len = ", len(trajectories[0].trajectory[:, 0]), "speed vector len = ", len(fwd_speed_self))
        generate_PFLAA_data(trajectories, PFLAA_file)


def generate_PFLAU_data(trajectories, PFLAU_file):
    safety_dist = 20  # 20m assumed as safety distance
    thr1 = 11
    thr2 = 16
    thr3 = 21
    traj_self = trajectories[0].trajectory

    for pt_idx in range(len(traj_self[:, 0]) - 1):
        # print("****************")
        intruder_list = []
        for trj_idx in range(1, len(trajectories)):  # index 0 is "Self". Indexes > 0 are "Others"
            traj_other = trajectories[trj_idx].trajectory
            fwd_speed_other = trajectories[trj_idx].fwd_speed
            vert_speed_other = trajectories[trj_idx].vert_speed
            TTC = 0
            for fwd_idx in range(pt_idx + 1, len(traj_self[:, 0])):
                TTC += 1
                # compute the "self" to "other" distance in future to evaluate if there will be risk of collision
                x_self = traj_self[fwd_idx, 0]
                y_self = traj_self[fwd_idx, 1]
                z_self = traj_self[fwd_idx, 2]
                x_other = traj_other[fwd_idx, 0]
                y_other = traj_other[fwd_idx, 1]
                z_other = traj_other[fwd_idx, 2]
                aircraft_to_aircraft_dist = np.sqrt((x_self - x_other) * (x_self - x_other) +
                                                    (y_self - y_other) * (y_self - y_other) +
                                                    (z_self - z_other) * (z_self - z_other))
                # print("aircraft_to_aircraft_dist = ", aircraft_to_aircraft_dist)
                if aircraft_to_aircraft_dist < safety_dist:
                    # compute the horizontal distance of "self" to "other" at current time
                    x_s = traj_self[pt_idx, 0]
                    y_s = traj_self[pt_idx, 1]
                    x_o = traj_other[pt_idx, 0]
                    y_o = traj_other[pt_idx, 1]
                    rel_hor_dist = np.sqrt((x_s - x_o) * (x_s - x_o) + (y_s - y_o) * (y_s - y_o))
                    # compute the vertical distance of "self" to "other" at current time
                    z_s = traj_self[pt_idx, 2]
                    z_o = traj_other[pt_idx, 2]
                    rel_vert_dist = z_o - z_s
                    # compute bearing
                    self_dir_x = traj_self[pt_idx + 1, 0] - x_s
                    self_dir_y = traj_self[pt_idx + 1, 1] - y_s
                    other_dir_x = traj_other[pt_idx, 0] - x_s
                    other_dir_y = traj_other[pt_idx, 1] - y_s
                    self_dir = np.array([self_dir_x, self_dir_y])
                    other_dir = np.array([other_dir_x, other_dir_y])
                    self_dir = self_dir / (np.sqrt(np.dot(self_dir, self_dir)) + 1e-6)
                    other_dir = other_dir / (np.sqrt(np.dot(other_dir, other_dir)) + 1e-6)
                    bearing_angle = int(round(np.degrees(np.arccos(np.dot(self_dir, other_dir)))))
                    # print("/////////////////")
                    # print("rel_hor_dist = ", rel_hor_dist, "rel_vert_dist = ", rel_vert_dist, "TTC = ", TTC)
                    # print(self_dir_x, self_dir_y, other_dir_x, other_dir_y, self_dir[0], self_dir[1], other_dir[0], other_dir[1])
                    # print("x_s ", x_s, " y_s ", y_s, " z_s ", z_s, " x_o ", x_o, " y_o ", y_o, " z_o ", z_o)
                    # print("bearing_angle ", bearing_angle)
                    # print(f"\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\")
                    intruder_PFLAU_info = PFLAU_Info()
                    intruder_PFLAU_info.ICAO_ID = get_ICAO_ID(trj_idx)
                    intruder_PFLAU_info.TTC = TTC
                    intruder_PFLAU_info.rel_vert_dist = rel_vert_dist
                    intruder_PFLAU_info.rel_hor_dist = rel_hor_dist
                    intruder_PFLAU_info.rel_bearing_angle = bearing_angle
                    ICAO_ID = get_ICAO_ID(trj_idx)
                    if TTC < thr1:
                        # print("A ", pt_idx, TTC, rel_vert_dist, rel_hor_dist, ICAO_ID)
                        intruder_PFLAU_info.alarm_level = 3
                        intruder_list.append(intruder_PFLAU_info)
                        break  # break here because the first point found in the trajectory has lowest TTC
                    if TTC < thr2:
                        # print("B ", pt_idx, TTC, rel_vert_dist, rel_hor_dist, ICAO_ID)
                        intruder_PFLAU_info.alarm_level = 2
                        intruder_list.append(intruder_PFLAU_info)
                        break  # break here because the first point found in the trajectory has lowest TTC
                    if TTC < thr3:
                        # print("C ", pt_idx, TTC, rel_vert_dist, rel_hor_dist, ICAO_ID)
                        intruder_PFLAU_info.alarm_level = 1
                        intruder_list.append(intruder_PFLAU_info)
                        break  # break here because the first point found in the trajectory has lowest TTC

        PFLAU_write(intruder_list, trajectories, PFLAU_file)
        # print("\n")


def PFLAU_write(intruder_list, trajectories, PFLAU_file):
    TTC_min = 2147483647
    idx_min = -1
    for i in range(len(intruder_list)):
        TTC_curr = intruder_list[i].TTC
        if TTC_curr < TTC_min:
            TTC_min = TTC_curr
            idx_min = i
    if idx_min > -1:
        # print("idx_min = ", idx_min)
        # print("Critical aircraft: ", intruder_list[idx_min].ICAO_ID)
        csv_string = "PFLAU,"
        csv_string += str(len(trajectories) - 1)  # RX
        csv_string += ","
        csv_string += "1,"  # TX
        csv_string += "1,"  # GPS
        csv_string += "1,"  # Power
        csv_string += str(intruder_list[idx_min].alarm_level)  # AlarmLevel
        csv_string += ","
        csv_string += str(int(round(intruder_list[idx_min].rel_bearing_angle)))  # RelativeBearing in degree
        csv_string += ","
        csv_string += "2,"  # AlarmType
        csv_string += str(int(round(intruder_list[idx_min].rel_vert_dist)))  # RelativeVertical
        csv_string += ","
        csv_string += str(int(round(intruder_list[idx_min].rel_hor_dist)))  # RelativeDistance
        csv_string += ","
        csv_string += str(intruder_list[idx_min].ICAO_ID)  # ID
        csv_string += "\n"
    else:
        # print("No critical aircraft found")
        csv_string = "PFLAU,"
        csv_string += str(len(trajectories) - 1)  # RX
        csv_string += ","
        csv_string += "1,"  # TX
        csv_string += "1,"  # GPS
        csv_string += "1,"  # Power
        csv_string += "0,"  # AlarmLevel
        csv_string += "0,"  # RelativeBearing
        csv_string += "0,"  # AlarmType
        csv_string += ","
        csv_string += ",\n"
    PFLAU_file.writelines(csv_string)



def generate_PFLAA_data(trajectories, PFLAA_file):
    safety_dist = 20  # 20m assumed as safety distance
    thr1 = 11
    thr2 = 16
    thr3 = 21
    traj_self = trajectories[0].trajectory
    fwd_speed_self = trajectories[0].fwd_speed
    vert_speed_self = trajectories[0].vert_speed

    for pt_idx in range(len(traj_self[:, 0]) - 1):
        # print("****************")
        intruder_list = []
        for trj_idx in range(1, len(trajectories)):  # index 0 is "Self". Indexes > 0 are "Others"
            traj_other = trajectories[trj_idx].trajectory
            fwd_speed_other = trajectories[trj_idx].fwd_speed[pt_idx]
            vert_speed_other = trajectories[trj_idx].vert_speed[pt_idx]
            intruder_PFLAA_info = PFLAA_Info()
            # compute the relative distance to north of "other" at current time --> fix me
            # ASSUMPTION: X axis of absolute frame points to North
            x_s = traj_self[pt_idx, 0]
            x_o = traj_other[pt_idx, 0]
            rel_north = x_o - x_s
            # compute the relative distance to east of "other" at current time --> fix me
            # ASSUMPTION: Y axis of absolute frame points to East
            y_s = traj_self[pt_idx, 1]
            y_o = traj_other[pt_idx, 1]
            rel_east = y_o - y_s
            # compute the vertical distance of "self" to "other" at current time --> OK
            z_s = traj_self[pt_idx, 2]
            z_o = traj_other[pt_idx, 2]
            rel_vert_dist = z_o - z_s
            ICAO_ID = get_ICAO_ID(trj_idx)
            intruder_PFLAA_info.ICAO_ID = ICAO_ID
            intruder_PFLAA_info.rel_vert_dist = rel_vert_dist
            intruder_PFLAA_info.relative_east = rel_east
            intruder_PFLAA_info.relative_north = rel_north
            intruder_PFLAA_info.track = int(np.round(np.arctan2((y_o - y_s), (x_o - x_s)) * 180 / np.pi))
            intruder_PFLAA_info.alarm_level = 0
            TTC = 0
            for fwd_idx in range(pt_idx + 1, len(traj_self[:, 0])):
                TTC += 1
                # compute the "self" to "other" distance in future to evaluate if there will be risk of collision
                x_self = traj_self[fwd_idx, 0]
                y_self = traj_self[fwd_idx, 1]
                z_self = traj_self[fwd_idx, 2]
                x_other = traj_other[fwd_idx, 0]
                y_other = traj_other[fwd_idx, 1]
                z_other = traj_other[fwd_idx, 2]
                aircraft_to_aircraft_dist = np.sqrt((x_self - x_other) * (x_self - x_other) +
                                                    (y_self - y_other) * (y_self - y_other) +
                                                    (z_self - z_other) * (z_self - z_other))
                # print("aircraft_to_aircraft_dist = ", aircraft_to_aircraft_dist)
                if aircraft_to_aircraft_dist < safety_dist:
                    if TTC < thr1:
                        # print("A ", pt_idx, TTC, rel_vert_dist, ICAO_ID)
                        intruder_PFLAA_info.alarm_level = 3
                        break  # break here because the first point found in the trajectory has lowest TTC
                    if TTC < thr2:
                        # print("B ", pt_idx, TTC, rel_vert_dist, ICAO_ID)
                        intruder_PFLAA_info.alarm_level = 2
                        break  # break here because the first point found in the trajectory has lowest TTC
                    if TTC < thr3:
                        # print("C ", pt_idx, TTC, rel_vert_dist, ICAO_ID)
                        intruder_PFLAA_info.alarm_level = 1
                        break  # break here because the first point found in the trajectory has lowest TTC
            # print("/////////////////")
            # print("rel_east_dist = ", rel_east, "rel_north_dist = ", rel_north, "TTC = ", TTC)
            # # print(self_dir_x, self_dir_y, other_dir_x, other_dir_y, self_dir[0], self_dir[1], other_dir[0], other_dir[1])
            # print("x_s ", x_s, " y_s ", y_s, " z_s ", z_s, " x_o ", x_o, " y_o ", y_o, " z_o ", z_o)
            # print(f"\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\")
            intruder_PFLAA_info.TTC = TTC
            intruder_list.append(intruder_PFLAA_info)

        PFLAA_write(intruder_list, fwd_speed_other, vert_speed_other, PFLAA_file)
        # print("\n")


def PFLAA_write(intruder_list, fwd_speed, vert_speed, PFLAU_file):
    """
    PFLAA,<AlarmLevel>,<RelativeNorth>,<RelativeEast>,<RelativeVertical>,<IDType>,<ID>,
    <Track>,<TurnRate>,<GroundSpeed>,<ClimbRate>,<AcftType>[,<NoTrack>[,<Source>,<RSSI>]]
    """
    for aircraft in intruder_list:
        csv_string = "PFLAA,"
        csv_string += str(aircraft.alarm_level)  # AlarmLevel
        csv_string += ","
        csv_string += str(int(round(aircraft.relative_north)))  # RelativeNorth
        csv_string += ","
        csv_string += str(int(round(aircraft.relative_east)))  # RelativeEast
        csv_string += ","
        csv_string += str(int(round(aircraft.rel_vert_dist)))  # RelativeVertical
        csv_string += ","
        csv_string += "1"  # IDType --> ICAO
        csv_string += ","
        csv_string += str(aircraft.ICAO_ID)  # ID
        csv_string += ","
        csv_string += str(aircraft.track)  # Track
        csv_string += ","
        csv_string += ""  # TurnRate --> empty
        csv_string += ","
        csv_string += str(fwd_speed)  # GroundSpeed
        csv_string += ","
        csv_string += str(vert_speed)  # ClimbRate
        csv_string += ","
        csv_string += "6"  # AcftType --> hard glider
        csv_string += ","
        csv_string += "0"  # NoTrack --> not set
        csv_string += ","
        csv_string += "0"  # Source FLARM
        csv_string += ","
        csv_string += ""  # RSSI --> empty
        csv_string += "\n"
        PFLAU_file.writelines(csv_string)


def get_ICAO_ID(idx):
    ICAO_base_addr_dec = int("300000", 16)  # Italian aircraft
    ICAO_ID_dec = ICAO_base_addr_dec + idx
    ICAO_ID = f'{ICAO_ID_dec:x}'  # hex(ICAO_ID_dec)
    return ICAO_ID.upper()
