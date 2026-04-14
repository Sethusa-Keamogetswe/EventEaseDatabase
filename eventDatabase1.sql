USE master;
ALTER DATABASE EventEaseDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE EventEaseDB;
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'EventEaseDB')
    DROP DATABASE EventEaseDB;
CREATE DATABASE EventEaseDB;

USE EventEaseDB;

-- Venue Table
CREATE TABLE Venue (
    VenueID INT IDENTITY(1,1) PRIMARY KEY,
    VenueName NVARCHAR(255) NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    Capacity INT NOT NULL CHECK (Capacity > 0),
    ImageUrl NVARCHAR(500) NULL
);

-- Event Table
CREATE TABLE Event (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(255) NOT NULL,
    EventDate DATETIME NOT NULL,
    Description NVARCHAR(1000) NULL,
    VenueID INT NULL, -- Venue may not be assigned initially
    FOREIGN KEY (VenueID) REFERENCES Venue(VenueID) ON DELETE SET NULL

);

-- Booking Table
CREATE TABLE Booking (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    VenueID INT NOT NULL,
    BookingDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (EventID) REFERENCES Event(EventID) ON DELETE CASCADE,
    FOREIGN KEY (VenueID) REFERENCES Venue(VenueID) ON DELETE CASCADE
);

-- Insert data into Venue table
INSERT INTO Venue (VenueName, Location, Capacity, ImageUrl)
VALUES 
('The staples center(Crypto.com Arena)', '1111 S Figueroa, Los Angeles', 20000, 'http://tile.loc.gov/image-services/iiif/service:pnp:highsm:24300:24341/full/pct:25/0/default.jpg'),
('The Rits-Carlton, Laguna Niguel', '1 Rits-Carlton Dr,Dana Point', 800, 'https://dynamic-media-cdn.tripadvisor.com/media/photo-o/20/96/47/f2/the-bluff-at-the-ritz.jpg'),
('The O2 Arena', 'Peninsula Square, London', 20000, 'https://populous.com/showcases/the-o2-london'),
('The Melbourne Convention and Exhibition centre', '1 Convention Centre Pl, South Wharf VIC', 5500, 'https://www.tripadvisor.com/Attraction_Review-g255100-d1419695-Reviews-Melbourne_Convention_and_Exhibition_Centre-Melbourne_Victoria.html'),
('The Banff Centre fir Arts and Creativity', '107 Tunnel Mountain Drive , Banff Alberta', 1500, 'https://www.tripadvisor.com/Hotel_Review-g154911-d2230839-Reviews-Banff_Centre_for_Arts_and_Creativity_Hotels-Banff_Banff_National_Park_Alberta.html');

-- Insert data into Event table
INSERT INTO Event (EventName, EventDate, Description, VenueID)
VALUES 
('Legends of Music: The Summer tour', '2026-06-15 10:00:00', 'A concert tour featuring rock and pop from the 80s, 90s and early 2000s.', 1),
('The Oceanfront Wedding Soiree', '2026-10-12 15:00:00', 'Celebration of the marriage in front of the Ocean.', 2),
('The Global Music Festival', '2026-11-03 18:00:00', 'A music festival featuring a blend of pop, electronic, and world music performances.', 3),
('The Future Tech Expo 2026', '2026-10-10 19:00:00', 'An exhibition that brings together the most innovative statrups, tech companies.', 4),
('Creative Minds: Leadership and innovation summit', '2026-07-22 15:00:00', 'A 3 day summit for creative proffesionals, entrepeneurs, and leaders in the arts.', 5);

-- Insert data into Booking table
INSERT INTO Booking (EventID, VenueID, BookingDate)
VALUES 
(1, 1, '2026-06-15 10:00:00'),
(2, 2, '2026-10-12 15:00:00'),
(3, 3, '2026-11-03 18:00:00'),
(4, 4, '2026-10-10 19:00:00'),
(5, 5, '2026-07-22 15:00:00');

-- Final Data Check
SELECT * FROM Booking;
SELECT * FROM Venue;
SELECT * FROM Event;

-- Dropping all Tables
DROP TABLE Booking;
DROP TABLE Event;
DROP TABLE Venue;
