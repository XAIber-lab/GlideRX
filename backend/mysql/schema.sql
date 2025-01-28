CREATE TABLE aircrafts (
    id INT AUTO_INCREMENT PRIMARY KEY,     -- Unique identifier for each aircraft
    name VARCHAR(255) NOT NULL,             -- Name of the aircraft
    registration_number VARCHAR(100),       -- Registration number (optional)
    manufacturer VARCHAR(255),              -- Manufacturer of the aircraft (optional)
    model VARCHAR(255)                      -- Model of the aircraft (optional)
);

CREATE TABLE trajectories (
    id INT AUTO_INCREMENT PRIMARY KEY,     -- Unique identifier for each trajectory
    aircraft_id INT,                       -- Foreign key to the aircraft table
    name VARCHAR(255),                      -- Name or description of the trajectory (e.g., flight number)
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP, -- Timestamp when the trajectory was created
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- Timestamp when the record was last updated
    FOREIGN KEY (aircraft_id) REFERENCES aircrafts(id)  -- Foreign key constraint
    ON DELETE CASCADE
);

CREATE TABLE state_vectors (
    id INT AUTO_INCREMENT PRIMARY KEY,     -- Unique identifier for each state vector
    trajectory_id INT NOT NULL,            -- Foreign key to the trajectories table
    latitude DECIMAL(9,6) NOT NULL,        -- Latitude (decimal format with 6 decimal places)
    longitude DECIMAL(9,6) NOT NULL,       -- Longitude (decimal format with 6 decimal places)
    altitude DECIMAL(10,2) NOT NULL,       -- Altitude in meters
    timestamp TIMESTAMP NOT NULL,          -- Timestamp when the state vector was recorded
    FOREIGN KEY (trajectory_id) REFERENCES trajectories(id) -- Foreign key constraint
    ON DELETE CASCADE
);

CREATE TABLE predicted_state_vectors (
    id INT AUTO_INCREMENT PRIMARY KEY,
    trajectory_id INT NOT NULL,
    latitude DECIMAL(9,6) NOT NULL,        -- Latitude (decimal format with 6 decimal places)
    longitude DECIMAL(9,6) NOT NULL,       -- Longitude (decimal format with 6 decimal places)
    altitude DECIMAL(10,2) NOT NULL,       -- Altitude in meters
    timestamp TIMESTAMP NOT NULL,          -- Timestamp when the state vector was recorded
    FOREIGN KEY (trajectory_id) REFERENCES trajectories(id) -- Foreign key constraint
    ON DELETE CASCADE
);
