/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.bazzana.controlpanel;

import com.bazzana.controlpanel.data.Segment;
import com.bazzana.controlpanel.data.StateVector;
import com.bazzana.controlpanel.data.Trajectory;
import com.gluonhq.maps.MapLayer;
import com.gluonhq.maps.MapPoint;
import javafx.geometry.Point2D;
import javafx.scene.Node;
import javafx.scene.shape.Line;
import com.bazzana.controlpanel.data.PositionAwareNode;

/**
 *
 * @author davide
 */
public class TrajectoryMapLayer extends MapLayer {
    
    private final Trajectory trajectory;
    
    public TrajectoryMapLayer(Trajectory trajectory) {
        this.trajectory = trajectory;
    }
    
    public void addNode(Node node) {
        this.getChildren().add(node);
        
        this.markDirty();
    }
    
    public void clear() {
        this.getChildren().clear();
    }

    @Override
    protected void layoutLayer() {
        System.out.println("Starting refresh for trajectory " + trajectory.getName());
        for (PositionAwareNode stateVector : trajectory.getStateVectors()) {
            updateNode(stateVector);
        }
        for (PositionAwareNode predictedStateVector : trajectory.getPredictedStateVectors()) {
            updateNode(predictedStateVector);
        }
    }
    
    private void updateNode(PositionAwareNode stateVector) {
        MapPoint point = stateVector.getMapPoint();
        Node icon = stateVector.getIcon();
        Point2D mapPoint = getMapPoint(point.getLatitude(), point.getLongitude());
        icon.setVisible(true);
        icon.setTranslateX(mapPoint.getX());
        icon.setTranslateY(mapPoint.getY());

        Segment pastSegment = stateVector.getPastSegment();
        if (pastSegment != null) {
            PositionAwareNode pastStateVector = pastSegment.getStartingPoint();

            Point2D pastMapPoint = getMapPoint(pastStateVector.getMapPoint().getLatitude(), pastStateVector.getMapPoint().getLongitude());
            Line line = pastSegment.getLine();
            line.setVisible(true);
            line.setStartX(pastMapPoint.getX());
            line.setStartY(pastMapPoint.getY());
            line.setEndX(mapPoint.getX());
            line.setEndY(mapPoint.getY());
        }
    }
}
