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
import javafx.collections.FXCollections;
import javafx.collections.ObservableList;
import javafx.geometry.Point2D;
import javafx.scene.Node;
import javafx.scene.shape.Line;

/**
 *
 * @author davide
 */
public class CustomMapLayer extends MapLayer {
    
    private final ObservableList<Trajectory> trajectories = FXCollections.observableArrayList();
    
    public CustomMapLayer() {
    }

    public void addTrajectory(Trajectory trajectory) {
        trajectories.add(trajectory);
        
        this.markDirty();
    }
    
    public void addNode(Node node) {
        this.getChildren().add(node);
        
        this.markDirty();
    }

    @Override
    protected void layoutLayer() {
        System.out.println("Starting refresh");
        for (Trajectory trajectory : trajectories) {
            System.out.println("TRAJECTORY");
            for (StateVector segmentPoint : trajectory.getStateVectors()) {
                System.out.println("SEGMENT");
                MapPoint point = segmentPoint.getMapPoint();
                Node icon = segmentPoint.getIcon();
                Point2D mapPoint = getMapPoint(point.getLatitude(), point.getLongitude());
                icon.setVisible(true);
                icon.setTranslateX(mapPoint.getX());
                icon.setTranslateY(mapPoint.getY());
                
                Segment pastSegment = segmentPoint.getPastSegment();
                if (pastSegment != null) {
                    StateVector pastStateVector = (StateVector) pastSegment.getStartingPoint();
                    
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
    }
}
