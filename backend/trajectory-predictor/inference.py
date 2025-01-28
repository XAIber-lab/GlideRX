import torch
import torch.nn as nn
import torch.utils
from LSTM import LSTMPredictor

model_path = "./models/GlideRX_LSTM.pt"

def infer(sample, steps_ahead=10):
    model = LSTMPredictor(n_hidden=512)
    # model.cuda()

    model.load_state_dict(torch.load(model_path, weights_only=True))
    model.eval()

    with torch.no_grad():
        sample = sample.type(torch.float32) # .cuda()
        output = model(sample, future=steps_ahead)
    return output

