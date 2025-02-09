import torch

# Multiplying by scale factor from meters to deg for Rieti (Italy)
scale_lat_m = 1 / (1.11 * 1e+05)
scale_long_m = 1 / (0.821 * 1e+05)

def invert_transform(input: torch.Tensor, base_latitude: float, base_longitude: float, base_altitude: float):
    scale_matrix = torch.diag(torch.tensor([scale_lat_m, scale_long_m, 1], dtype=input.dtype))
    if input.is_cuda:
        scale_matrix = scale_matrix.cuda()
    input = torch.matmul(input, scale_matrix)
    base_state_vector = torch.Tensor([base_latitude, base_longitude, base_altitude]).repeat(input.shape[0], 1)
    return input + base_state_vector
