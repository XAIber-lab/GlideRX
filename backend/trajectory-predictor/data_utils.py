import torch
import numpy as np
import math

# Multiplying by scale factor from deg to decameters for Rieti (Italy)
scale_lat_dm = 1.11 * 1e+04
scale_long_dm = 0.821 * 1e+04

def transform(input: torch.Tensor):
    base_lat = input[0][0]
    base_long = input[0][1]
    base_alt = input[0][2]
    base_state_vector = torch.Tensor([base_lat, base_long, base_alt]).repeat(input.shape[0], 1)
    input = input - base_state_vector
    # Multiplying by scale factor from deg to decameters for Rieti (Italy)
    scale_matrix = torch.diag(torch.tensor([scale_lat_dm, scale_long_dm, 1], dtype=input.dtype))
    if input.is_cuda:
        scale_matrix = scale_matrix.cuda()
    return torch.matmul(input, scale_matrix)


def invert_transform(input: torch.Tensor, base_latitude: float, base_longitude: float, base_altitude: float):
    # Multiplying by scale factor from decameters to deg for Rieti (Italy)
    scale_matrix = torch.diag(torch.tensor([1/scale_lat_dm, 1/scale_long_dm, 1], dtype=input.dtype))
    if input.is_cuda:
        scale_matrix = scale_matrix.cuda()
    input = torch.matmul(input, scale_matrix)
    base_state_vector = torch.Tensor([base_latitude, base_longitude, base_altitude]).repeat(input.shape[0], 1)
    return input + base_state_vector    
    
    
