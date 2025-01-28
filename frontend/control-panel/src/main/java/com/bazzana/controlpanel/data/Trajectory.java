/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.bazzana.controlpanel.data;

import com.bazzana.controlpanel.CustomMapLayer;
import com.bazzana.controlpanel.TrajectoryMapLayer;
import com.gluonhq.maps.MapPoint;
import javafx.scene.Node;
import jakarta.persistence.*;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.ListIterator;
import javafx.scene.paint.Color;

/**
 *
 * @author davide
 */
@Entity
@Table(name = "trajectories")
public class Trajectory {
    
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    
    @ManyToOne
    @JoinColumn(name = "aircraft_id")
    private Aircraft aircraft;
    
    @Column(name = "name")
    private String name;
    
    @OneToMany(mappedBy = "trajectory", fetch = FetchType.EAGER, cascade = CascadeType.ALL)
    private List<StateVector> stateVectors = new ArrayList<>();
    
    @OneToMany(mappedBy = "trajectory", fetch = FetchType.EAGER, cascade = CascadeType.ALL)
    private List<PredictedStateVector> predictedStateVectors = new ArrayList<>();
    
    @Column(name = "created_at", updatable = false)
    private LocalDateTime createdAt;
    
    @Column(name = "updated_at")
    private LocalDateTime updatedAt;
    
    @Transient
    private CustomMapLayer mapLayer;
    
    @Transient
    private TrajectoryMapLayer trajectoryMapLayer;
    
    public Trajectory() {
        this.trajectoryMapLayer = new TrajectoryMapLayer(this);
    }
    
    public Trajectory(CustomMapLayer mapLayer) {
        this.mapLayer = mapLayer;
    }
    
    public void addStateVector(MapPoint mapPoint, Node icon) {
        StateVector point = new StateVector(mapPoint, icon);
        if (stateVectors.isEmpty()) {
            stateVectors.add(point);
            
            this.mapLayer.addNode(icon);
        }
        else {
            StateVector lastPoint = stateVectors.get(stateVectors.size() - 1);
            Segment newSegment = new Segment(lastPoint, point, Color.BLUE, 3);
            point.setPastSegment(newSegment);
            
            stateVectors.add(point);
            this.mapLayer.addNode(icon);
            this.mapLayer.addNode(newSegment.getLine());
        }
    }
    
    @PostLoad
    public void updateSegments() {
        ListIterator<StateVector> svIterator = stateVectors.listIterator();
        StateVector lastStateVector = null;
        while (svIterator.hasNext()) {
            StateVector stateVector = svIterator.next();
            this.trajectoryMapLayer.addNode(stateVector.getIcon());
            
            if (lastStateVector != null) {
                Segment newSegment = new Segment(lastStateVector, stateVector, Color.BLUE, 3);
                stateVector.setPastSegment(newSegment);
                this.trajectoryMapLayer.addNode(newSegment.getLine());
            }
            
            if (!svIterator.hasNext() && !predictedStateVectors.isEmpty()) {
                PredictedStateVector firstPredictedStateVector = predictedStateVectors.get(0);
                Segment newSegment = new Segment(stateVector, firstPredictedStateVector, Color.RED, 3);
                firstPredictedStateVector.setPastSegment(newSegment);
                this.trajectoryMapLayer.addNode(newSegment.getLine());
            }
            
            lastStateVector = stateVector;
        }
        
        ListIterator<PredictedStateVector> predictedSVIterator = predictedStateVectors.listIterator();
        PredictedStateVector lastPredictedStateVector = null;
        while (predictedSVIterator.hasNext()) {
            PredictedStateVector predictedStateVector = predictedSVIterator.next();
            this.trajectoryMapLayer.addNode(predictedStateVector.getIcon());
            
            if (lastPredictedStateVector != null) {
                Segment newSegment = new Segment(lastPredictedStateVector, predictedStateVector, Color.RED, 3);
                predictedStateVector.setPastSegment(newSegment);
                this.trajectoryMapLayer.addNode(newSegment.getLine());
            }
            
            lastPredictedStateVector = predictedStateVector;
        }
    }
    
    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public Aircraft getAircraft() {
        return aircraft;
    }

    public void setAircraft(Aircraft aircraft) {
        this.aircraft = aircraft;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public List<StateVector> getStateVectors() {
        return stateVectors;
    }

    public void setStateVectors(List<StateVector> stateVectors) {
        this.stateVectors = stateVectors;
    }
    
    public List<PredictedStateVector> getPredictedStateVectors() {
        return predictedStateVectors;
    }

    public void setPredictedStateVectors(List<PredictedStateVector> predictedStateVectors) {
        this.predictedStateVectors = predictedStateVectors;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(LocalDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }

    public CustomMapLayer getMapLayer() {
        return mapLayer;
    }

    public void setMapLayer(CustomMapLayer mapLayer) {
        this.mapLayer = mapLayer;
    }
    
    public TrajectoryMapLayer getTrajectoryMapLayer() {
        return trajectoryMapLayer;
    }
}



