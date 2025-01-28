import torch


def invert_transform(input: torch.Tensor, base_latitude: float, base_longitude: float, base_altitude: float):
    scale_matrix = torch.diag(torch.tensor([1e-05, 1e-05, 1], dtype=input.dtype))
    if input.is_cuda:
        scale_matrix = scale_matrix.cuda()
    input = torch.matmul(input, scale_matrix)
    base_state_vector = torch.Tensor([base_latitude, base_longitude, base_altitude]).repeat(input.shape[0], 1)
    return input + base_state_vector
