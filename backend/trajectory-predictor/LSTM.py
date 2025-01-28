import torch
import torch.nn as nn
import numpy as np
from einops import rearrange


class LSTMPredictor(nn.Module):
    def __init__(self, n_hidden=64):
        super(LSTMPredictor, self).__init__()
        self.n_hidden = n_hidden
        # lstm1, lstm2, linear
        self.lstm1 = nn.LSTMCell(3, self.n_hidden)
        # self.lstm2 = nn.LSTMCell(self.n_hidden, self.n_hidden)
        self.linear = nn.Sequential(
            nn.Linear(in_features=self.n_hidden, out_features=3), 
        )


    def forward(self, x, future=0):
        outputs = []
        n_samples = x.size(0)

        h_t1 = torch.zeros(n_samples, self.n_hidden, dtype=torch.float32).cuda()
        c_t1 = torch.zeros(n_samples, self.n_hidden, dtype=torch.float32).cuda()
        # h_t2 = torch.zeros(n_samples, self.n_hidden, dtype=torch.float32).cuda()
        # c_t2 = torch.zeros(n_samples, self.n_hidden, dtype=torch.float32).cuda()

        for input_t in x.split(1, dim=1):
            input_t = torch.squeeze(input_t, dim=1)
            # idx = torch.from_numpy(np.array([1, 3, 5]))
            # input_t = input_t[idx]
            h_t1, c_t1 = self.lstm1(input_t, (h_t1, c_t1))
            # h_t2, c_t2 = self.lstm2(h_t1, (h_t2, c_t2))
            output = self.linear(h_t1)
            outputs.append(output)

        for i in range(future):
            h_t1, c_t1 = self.lstm1(output, (h_t1, c_t1))
            # h0_t2, c_t2 = self.lstm2(h_t1, (h_t2, c_t2))
            output = self.linear(h_t1)
            outputs.append(output)
 
        outputs = torch.stack(outputs, dim=0)
        outputs = rearrange(outputs, "s b n -> b s n")
        return outputs
