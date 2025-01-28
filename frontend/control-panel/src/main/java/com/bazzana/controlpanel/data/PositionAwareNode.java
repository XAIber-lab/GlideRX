/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Interface.java to edit this template
 */
package com.bazzana.controlpanel.data;

import com.gluonhq.maps.MapPoint;
import javafx.scene.Node;

/**
 *
 * @author davide
 */
public interface PositionAwareNode {
    MapPoint getMapPoint();
    Segment getPastSegment();
    Node getIcon();
}
