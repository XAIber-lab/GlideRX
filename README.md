# GlideRX

![morphing](https://github.com/user-attachments/assets/1db09122-a414-475e-87ac-a0e83b4320d0)

This repository contains the code related to the paper: "_GlideRX: Enhancing Situation Awareness for Collision Prevention in Glider Flight through Extended Reality_".

Gliders are a specific type of aircraft made for soaring. The principle behind soaring is to stay afloat using only the energy the atmosphere provides. Glider pilots are confronted with several challenges specific to this kind of flight, the major one being the need to maintain adequate separation from other gliders while still operating the aircraft effectively and safely.
In this paper, we present an XR intelligent interface, GlideRX, to help pilots manage traffic awareness and prevent mid-air collisions. It allows for better support of situational awareness with respect to
current collision warning systems currently available for gliders.
The integration of data readings about surrounding aircraft, coupled with predictive algorithms on their trajectories and directions and a morphable representation of visual aids to help identify threats, enhances the situational awareness of the pilot and the management of risky conditions.
We tested GlideRX efficacy and effectiveness through multiple evaluation activities, from a usage scenario to a quantitative task-driven experiment with real pilots in simulated conditions.
Results show that GlideRX can effectively support a glider pilot in safely managing their flight during the increasing presence of air traffic.

## Prerequisites
In order to run the code in this repository you will need the following dependencies:
- [Maven](https://maven.apache.org/)
- [Docker](https://www.docker.com/)

## Backend
The `backend/` folder contains the implementation of the FLARM parser, trajectory manager and trajectory predictor. In order to run these components together you can run the _docker compose_:
`/path/to/GlideRX/backend$ docker compose up`.

## Frontend
The `frontend/` folder contains two frontend application: _control-panel_ and _simulator_. _control-panel_ is a JavaFX application which showcases the functioning of the fuctionalities of the backend. _simulator_ is a Unity application which can be used to test the actual GlideRX user interface as experienced by the pilot in a simulated environment.

### Control Panel
To launch the application, run the command: `path/to/GlideRX/frontend/control-panel$ mvn clean javafx:run`
