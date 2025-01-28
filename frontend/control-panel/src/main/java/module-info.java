module com.bazzana.gliderxcontrolpanel {
    requires javafx.controls;
    requires javafx.fxml;
    requires javafx.base;

    opens com.bazzana.controlpanel to javafx.fxml;
    exports com.bazzana.controlpanel;
    requires com.gluonhq.maps;
    requires java.sql;
    requires jakarta.persistence;
    requires org.hibernate.orm.core;
    requires com.rabbitmq.client;
    
    exports com.bazzana.controlpanel.data to org.hibernate.orm.core;
    opens com.bazzana.controlpanel.data to org.hibernate.orm.core;
}
