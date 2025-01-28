# GlideRX

This repository contains the code related to the paper: "_GlideRX: Enhancing Situation Awareness for Collision Prevention in Glider Flight through Extended Reality_".
The repository is composed of two main parts: `backend/` and `frontend/`.

## Backend
The `backend/` folder contains the implementation of the FLARM parser, trajectory manager and trajectory predictor. These components can be run together using `docker compose`. For more information, please refer to the `README.md` located under `GlideRX/backend/README.md`.

## Frontend
The `frontend/` folder contains two frontend application: _control-panel_ and _simulator_. _control-panel_ is a JavaFX application which showcases the functioning of the fuctionalities of the backend. _simulator_ is a Unity application which can be used to test the actual GlideRX user interface as experienced by the pilot in a simulated environment.