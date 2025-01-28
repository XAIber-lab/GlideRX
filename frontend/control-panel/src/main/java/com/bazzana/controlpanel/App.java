package com.bazzana.controlpanel;

import com.bazzana.controlpanel.data.DatabaseUpdater;
import com.bazzana.controlpanel.data.Trajectory;
import javafx.application.Application;
import javafx.scene.Scene;
import javafx.stage.Stage;
import com.gluonhq.maps.MapView;
import com.gluonhq.maps.MapPoint;
import com.rabbitmq.client.Channel;
import com.rabbitmq.client.Connection;

import java.io.IOException;
import javafx.collections.ListChangeListener;
import javafx.collections.ObservableList;
import javafx.geometry.Orientation;
import javafx.scene.control.Label;
import javafx.scene.control.SplitPane;
import javafx.scene.layout.BorderPane;
import javafx.scene.control.TextArea;
import javafx.scene.layout.VBox;
import com.rabbitmq.client.ConnectionFactory;
import com.rabbitmq.client.DeliverCallback;
import java.sql.DriverManager;
import java.sql.SQLException;
import javafx.application.Platform;
import javafx.concurrent.Task;
import javafx.scene.control.Alert;
import javafx.scene.control.ProgressIndicator;
import javafx.stage.Modality;
import javafx.stage.StageStyle;

/**
 * JavaFX App
 */
public class App extends Application {

    private DatabaseUpdater dbUpdater = null;
    private ObservableList<Trajectory> trajectories = null;
    
    private final String QUEUE_NAME = "frontend";
    private Connection connection;
    private Channel channel;
    private TextArea FLARMLog;
    private MapView mapView;
    
    private Thread checkServers;
    
    @Override
    public void start(Stage stage) throws IOException {
        /*
        try (Connection connection = DatabaseUtil.getConnection()) {
            System.out.println("Connected to the database!");
        } catch (SQLException e) {
            e.printStackTrace();
        }
        */
        /*
        EntityManager em = DatabaseHelper.getEntityManager();
        List<Trajectory> trajectories = null;
        List<TrajectoryMapLayer> trajectoriesMapLayers = new ArrayList<>();
        try {
            em.getTransaction().begin();
            // Create JPQL query
            Query query = em.createQuery("SELECT t FROM Trajectory t");
            trajectories = query.getResultList();
            em.getTransaction().commit();
        } catch (Exception e) {
            if (em.getTransaction().isActive()) {
                em.getTransaction().rollback();
            }
            e.printStackTrace();
        } finally {
            DatabaseHelper.closeEntityManager();
        }
        for (Trajectory trajectory: trajectories) {
            System.out.println(trajectory.getName());
            trajectoriesMapLayers.add(trajectory.getTrajectoryMapLayer());
            for (PredictedStateVector predictedStateVector: trajectory.getPredictedStateVectors())
                System.out.println(predictedStateVector);
        }
        */
        
        MapPoint LIQN = new MapPoint(42.4272, 12.8517);
        mapView = new MapView();
        mapView.setCenter(LIQN);
        mapView.setZoom(12);
        /*
        for (TrajectoryMapLayer trajectoryMapLayer : trajectoriesMapLayers) {
            mapView.addLayer(trajectoryMapLayer);
        }
        */
        
        VBox rightPanel = new VBox();
        rightPanel.setMaxWidth(250);
        rightPanel.setMinWidth(200);
        rightPanel.getStyleClass().add("right-panel");
        rightPanel.getChildren().add(createLabelValuePair("THREAT LEVEL:", "LOW"));
        rightPanel.getChildren().add(createLabelValuePair("TTC:", "27.5s"));
        rightPanel.getChildren().add(createLabelValuePair("THREAT ID:", "3"));
        
        FLARMLog = new TextArea();
        FLARMLog.setEditable(false);
        FLARMLog.setWrapText(true);
        FLARMLog.getStyleClass().add("FLARMLog");
        FLARMLog.appendText(
"   _____   _   _       _          _____   __   __\n" +
"  / ____| | | (_)     | |        |  __ \\  \\ \\ / /\n" +
" | |  __  | |  _    __| |   ___  | |__) |  \\ V / \n" +
" | | |_ | | | | |  / _` |  / _ \\ |  _  /    > <  \n" +
" | |__| | | | | | | (_| | |  __/ | | \\ \\   / . \\ \n" +
"  \\_____| |_| |_|  \\__,_|  \\___| |_|  \\_\\ /_/ \\_\\\n" +
"                                                 \n" +
"");
        FLARMLog.appendText("> Welcome to the Messages Log!\n");
        
        BorderPane mapAndStats = new BorderPane();
        mapAndStats.setCenter(mapView);
        mapAndStats.setRight(rightPanel);
        SplitPane root = new SplitPane();
        root.setOrientation(Orientation.VERTICAL);
        root.setDividerPositions(0.65);
        root.getItems().addAll(mapAndStats, FLARMLog);
        // root.setBottom(term);
        
        Scene scene = new Scene(root, 800, 600);
        scene.getStylesheets().add(getClass().getResource("control-panel-styles.css").toExternalForm());
        rightPanel.prefWidthProperty().bind(scene.widthProperty().multiply(0.2));
        // FLARMLog.prefHeightProperty().bind(scene.heightProperty().multiply(0.1));
        
        stage.setTitle("GlideRX Control Panel");
        stage.setScene(scene);
        stage.show();
        
        showBlockingDialogWithTask(stage);
        // startRabbitMQConsumer();
        // dbUpdater.startQuerying();
        /*
        new Thread(() -> {
            try {
                Thread.sleep(2000);
                javafx.application.Platform.runLater(() -> FLARMLog.appendText("> New log entry: Task started.\n"));
                Thread.sleep(2000);
                javafx.application.Platform.runLater(() -> FLARMLog.appendText("> New log entry: Task completed.\n"));
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }).start();
        */
    }
    
    private void showBlockingDialogWithTask(Stage owner) {
        Stage dialog = new Stage();
        dialog.initStyle(StageStyle.UTILITY);
        dialog.initModality(Modality.APPLICATION_MODAL);
        dialog.initOwner(owner);
        dialog.setTitle("Please Wait");

        ProgressIndicator progressIndicator = new ProgressIndicator();
        Label messageLabel = new Label("Checking servers...");
        VBox vbox = new VBox(10, progressIndicator, messageLabel);
        vbox.setStyle("-fx-padding: 20; -fx-alignment: center;");
        dialog.setScene(new Scene(vbox, 300, 150));
        dialog.setOnCloseRequest(event -> {
            System.out.println("Application is shutting down...");
            Platform.exit();
        });

        Task<Void> task = new Task<>() {
            @Override
            protected Void call() throws Exception {
                // Simulate a server check
                boolean mySQLConnection = checkMySQLConnection();
                while (!mySQLConnection) {
                    mySQLConnection = checkMySQLConnection();
                    Thread.sleep(2000);
                }
                boolean myRabbitMQConnection = checkRabbitMQConnection();
                while (!myRabbitMQConnection) {
                    myRabbitMQConnection = checkRabbitMQConnection();
                    Thread.sleep(2000);
                }
                return null;
            }

            @Override
            protected void succeeded() {
                // Close dialog and update status on UI thread
                Platform.runLater(() -> {
                    dialog.close();
                    // statusLabel.setText("Status: Servers are reachable!");
                    dbUpdater = new DatabaseUpdater();
                    trajectories = dbUpdater.getTrajectories();

                    trajectories.addListener((ListChangeListener<Trajectory>) change -> {
                        while (change.next()) {
                            if (change.wasAdded()) {
                                System.out.println("Added: " + change.getAddedSubList());
                                for (Trajectory trajectory: change.getAddedSubList()) {
                                    mapView.addLayer(trajectory.getTrajectoryMapLayer());
                                }
                            }
                            if (change.wasRemoved()) {
                                System.out.println("Removed: " + change.getRemoved());
                                for (Trajectory trajectory: change.getRemoved()) {
                                    System.out.println("Cleaning resources...");
                                    trajectory.getTrajectoryMapLayer().clear();
                                    mapView.removeLayer(trajectory.getMapLayer());
                                }
                            }
                            if (change.wasUpdated()) {
                                System.out.println("Updated at index: " + change.getFrom());
                            }
                        }
                    });
                    
                    startRabbitMQConsumer();
                    dbUpdater.startQuerying();
                });
            }

            @Override
            protected void failed() {
                // Close dialog and show an error message
                Platform.runLater(() -> {
                    dialog.close();
                    // statusLabel.setText("Status: Failed to reach servers.");
                });
            }
        };

        dialog.show();
        checkServers = new Thread(task);
        checkServers.setDaemon(true);
        checkServers.start();
    }

    private boolean checkMySQLConnection() {
        String url = "jdbc:mysql://localhost:3306/gliderx";
        String user = "root";
        String password = "my-secret-pw";

        try (java.sql.Connection conn = DriverManager.getConnection(url, user, password)) {
            return true; // Connection successful
        } catch (SQLException e) {
            e.printStackTrace();
            return false; // Connection failed
        }
    }

    private boolean checkRabbitMQConnection() {
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost("localhost");
        factory.setPort(5672);

        try (Connection connection = factory.newConnection()) {
            return true; // Connection successful
        } catch (Exception e) {
            e.printStackTrace();
            return false; // Connection failed
        }
    }

    private void showAlert(String title, String content, Alert.AlertType type) {
        Alert alert = new Alert(type);
        alert.setTitle(title);
        alert.setHeaderText(null);
        alert.setContentText(content);
        alert.showAndWait();
    }
    
    private void startRabbitMQConsumer() {
        new Thread(() -> {
            try {
                ConnectionFactory factory = new ConnectionFactory();
                factory.setHost("localhost");
                factory.setUsername("guest");
                factory.setPassword("guest");

                connection = factory.newConnection();
                channel = connection.createChannel();

                channel.queueDeclare(QUEUE_NAME, false, false, false, null);

                DeliverCallback deliverCallback = (consumerTag, delivery) -> {
                    String message = new String(delivery.getBody(), "UTF-8");

                    Platform.runLater(() -> {
                        FLARMLog.appendText("> " + message + "\n");
                    });
                };

                channel.basicConsume(QUEUE_NAME, true, deliverCallback, consumerTag -> {
                    Platform.runLater(() -> {
                        FLARMLog.appendText("Consumer canceled: " + consumerTag + "\n");
                    });
                });

            } catch (Exception e) {
                e.printStackTrace();
                Platform.runLater(() -> {
                    FLARMLog.appendText("Error: " + e.getMessage() + "\n");
                });
            }
        }).start();
    }
    
    @Override
    public void stop() {
        System.out.println("Closing...");
        System.out.println("dbUpdater: " + dbUpdater==null);
        
        if (checkServers != null && checkServers.isAlive()) {
            checkServers.interrupt();
        }
        
        if (dbUpdater != null) {
            dbUpdater.stopQuerying();
            dbUpdater.close();
        }
        try {
            // Clean up RabbitMQ resources when the application closes
            if (channel != null && channel.isOpen()) {
                channel.close();
            }
            if (connection != null && connection.isOpen()) {
                connection.close();
            } 
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
    
    private VBox createLabelValuePair(String labelText, String valueText) {
        // Create label
        Label label = new Label(labelText);
        label.getStyleClass().add("label");

        // Create value
        Label value = new Label(valueText);
        value.getStyleClass().add("value-label");

        // Create HBox for the pair
        VBox vbox = new VBox(10); // spacing between label and value
        vbox.getStyleClass().add("stats");
        vbox.getChildren().addAll(label, value);

        return vbox;
    }
    
    public static void main(String[] args) {
        launch();
    }

}