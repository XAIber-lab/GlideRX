/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.bazzana.controlpanel.data;

import javafx.application.Platform;
import javafx.collections.FXCollections;
import javafx.collections.ObservableList;

import jakarta.persistence.EntityManager;
import jakarta.persistence.EntityManagerFactory;
import jakarta.persistence.Persistence;
import jakarta.persistence.Query;
import java.util.List;
import java.util.concurrent.Executors;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;

/**
 *
 * @author davide
 */
public class DatabaseUpdater {
    private final EntityManagerFactory emf;
    private final ObservableList<Trajectory> trajectories = FXCollections.observableArrayList();
    private final ScheduledExecutorService scheduler = Executors.newScheduledThreadPool(1);

    public DatabaseUpdater() {
        emf = Persistence.createEntityManagerFactory("GlideRXPersistenceUnit");
    }

    public ObservableList<Trajectory> getTrajectories() {
        return trajectories;
    }

    public void startQuerying() {
        scheduler.scheduleAtFixedRate(() -> {
            List<Trajectory> newTrajectories = fetchTrajectories();
            Platform.runLater(() -> updateUI(newTrajectories));
        }, 0, 1, TimeUnit.SECONDS);
    }

    private List<Trajectory> fetchTrajectories() {
        EntityManager em = emf.createEntityManager();
        try {
            Query query = em.createQuery("SELECT t FROM Trajectory t");
            return query.getResultList();
        } finally {
            em.close();
        }
    }

    private void updateUI(List<Trajectory> newTrajectories) {
        trajectories.setAll(newTrajectories); // Updates the ObservableList, which updates the UI
    }

    public void stopQuerying() {
        scheduler.shutdown();
    }
    
    public void close() {
        if (emf != null && emf.isOpen()) {
            emf.close();
        }
    }
}

