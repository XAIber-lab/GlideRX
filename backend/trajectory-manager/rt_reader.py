import torch


class Reader:

    def __init__(self, n_intruders, self_position_file_name, pflaa_file_name):
        print(f'n_intruders: {n_intruders}')
        self.n_intruders = n_intruders
        self.self_position_file_name = self_position_file_name
        self.pflaa_file_name = pflaa_file_name
        self.msg_idx = 0
        with open(self_position_file_name, "r") as trajectory_self:
            # Do not count the heading
            self.n_state_vectors = sum(1 for _ in trajectory_self) - 1
            print(f'n_state_vectors: {self.n_state_vectors}')

    def get_next_message(self):
        pflaa_messages = []
        positions = []
        with open(self.self_position_file_name) as trajectory_self, open(self.pflaa_file_name) as pflaa:
            heading_self = next(trajectory_self)
            heading_pflaa = next(pflaa)
            self_X = 0
            self_Y = 0
            self_Z = 0
            idx = 0
            for self_line in trajectory_self:
                if idx < self.msg_idx:
                    idx += 1
                    continue
                self_line.strip()
                self_X, self_Y, self_Z, Q0, Q1, Q2, Q3 = self_line.split(',')
                positions.append(torch.tensor([float(self_X), float(self_Y), float(self_Z)]))
                break
            for i in range(self.n_intruders):
                other_idx = 0
                for pflaa_line in pflaa:
                    if other_idx < self.msg_idx:
                        other_idx += 1
                        continue
                    if not pflaa_line:
                        break
                    pflaa_line.strip()
                    pflaa_messages.append(pflaa_line.strip())
                    name, AlarmLev, RelNorth, RelEast, RelVert, IDType, ID, \
                        trk, TurnR, GroundSpeed, ClimbRate, ActfTy, NoTrk, Source, RSSI = \
                            pflaa_line.split(',')
                    other_X = float(self_X) + float(RelNorth)
                    other_Y = float(self_Y) + float(RelEast)
                    other_Z = float(self_Z) + float(RelVert)
                    positions.append(torch.tensor([float(other_X), float(other_Y), float(other_Z)]))
                    break
            self.msg_idx += 1
        positions = torch.stack(positions)
        return pflaa_messages, positions

    def has_next(self):
        return self.n_state_vectors - self.msg_idx > 0

