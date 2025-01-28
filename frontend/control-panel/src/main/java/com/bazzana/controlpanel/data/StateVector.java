/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.bazzana.controlpanel.data;

import com.gluonhq.maps.MapPoint;
import jakarta.persistence.Entity;
import jakarta.persistence.*;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import javafx.scene.Node;
import javafx.scene.paint.Color;
import javafx.scene.shape.Circle;

/**
 *
 * @author davide
 */
@Entity
@Table(name = "state_vectors")
public class StateVector implements PositionAwareNode {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private int id;
    
    @ManyToOne
    @JoinColumn(name = "trajectory_id", nullable = false)
    private Trajectory trajectory;
    
    @Column(name = "latitude", precision = 9, scale = 6, nullable = false)
    private BigDecimal latitude;
    
    @Column(name = "longitude", precision = 9, scale = 6, nullable = false)
    private BigDecimal longitude;
    
    @Column(name = "altitude", precision = 10, scale = 2, nullable = false)
    private BigDecimal altitude;
    
    @Column(name = "timestamp", updatable = false)
    private LocalDateTime timestamp;
    
    @Transient
    private MapPoint mapPoint;
    @Transient
    private Node icon;
    @Transient
    private Segment pastSegment;
    
    public StateVector() {
        
    }
    
    public StateVector(MapPoint mapPoint, Node icon) {
        this.mapPoint = mapPoint;
        this.icon = icon;
    }
    
    @PostLoad
    public void updateMapPoint() {
        this.mapPoint = new MapPoint(this.latitude.doubleValue(), this.longitude.doubleValue());
        this.icon = new Circle(3, Color.BLUE);
    }
    
    public int getId() {
        return id;
    }

    public void setId(int id) {
        this.id = id;
    }

    public Trajectory getTrajectory() {
        return trajectory;
    }

    public void setTrajectory(Trajectory trajectory) {
        this.trajectory = trajectory;
    }

    public BigDecimal getLatitude() {
        return latitude;
    }

    public void setLatitude(BigDecimal latitude) {
        this.latitude = latitude;
    }

    public BigDecimal getLongitude() {
        return longitude;
    }

    public void setLongitude(BigDecimal longitude) {
        this.longitude = longitude;
    }

    public BigDecimal getAltitude() {
        return altitude;
    }

    public void setAltitude(BigDecimal altitude) {
        this.altitude = altitude;
    }

    public LocalDateTime getTimestamp() {
        return timestamp;
    }

    public void setTimestamp(LocalDateTime timestamp) {
        this.timestamp = timestamp;
    }
    
    @Override
    public MapPoint getMapPoint() {
        return mapPoint;
    }

    @Override
    public Node getIcon() {
        return icon;
    }

    @Override
    public Segment getPastSegment() {
        return pastSegment;
    }

    public void setPastSegment(Segment segment) {
        this.pastSegment = segment;
    }
    
    @Override
    public String toString() {
        return "latitude: " + latitude + "\tlongitude: " + longitude + "\taltitude: " + altitude;
    }
}
