/*
 * Click nbfs://nbhost/SystemFileSystem/Templates/Licenses/license-default.txt to change this license
 * Click nbfs://nbhost/SystemFileSystem/Templates/Classes/Class.java to edit this template
 */
package com.bazzana.controlpanel.data;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;

/**
 *
 * @author davide
 */
public class DatabaseUtil {
    private static final String URL = "jdbc:mysql://localhost:3306/gliderx";
    private static final String USER = "root";
    private static final String PASSWORD = "my-secret-pw";

    public static Connection getConnection() throws SQLException {
        return DriverManager.getConnection(URL, USER, PASSWORD);
    }
}
