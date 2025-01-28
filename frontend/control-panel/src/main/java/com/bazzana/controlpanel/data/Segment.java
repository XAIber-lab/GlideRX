/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.bazzana.controlpanel.data;

import javafx.scene.paint.Color;
import javafx.scene.shape.Line;

/**
 *
 * @author davide
 */

public class Segment extends Line {
    private PositionAwareNode startingPoint;
    private PositionAwareNode endingPoint;
    private Line line;

    public Segment(PositionAwareNode startingPoint, PositionAwareNode endingPoint, Color color, double thickness) {
        this.startingPoint = startingPoint;
        this.endingPoint = endingPoint;
        
        this.line = new Line(
                startingPoint.getMapPoint().getLongitude(),
                startingPoint.getMapPoint().getLatitude(),
                endingPoint.getMapPoint().getLongitude(),
                endingPoint.getMapPoint().getLatitude());
        this.line.setStroke(color); // Set line color
        this.line.setStrokeWidth(thickness); // Set line thickness
    }
    
    public Line getLine() {
        return line;
    }
    
    public PositionAwareNode getStartingPoint() {
        return startingPoint;
    }
    
    public PositionAwareNode getEndingPoint() {
        return endingPoint;
    }
}
