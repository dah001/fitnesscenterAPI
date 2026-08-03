-- ============================================================
-- FitnessCenter – Test Data (100+ rows per entity)
-- ============================================================

USE fitnesscenter;

-- ─────────────────────────────────────────────────────────────
-- Locations (10)
-- ─────────────────────────────────────────────────────────────
INSERT INTO Location (City) VALUES
('Copenhagen'),('Aarhus'),('Odense'),('Aalborg'),('Esbjerg'),
('Randers'),('Kolding'),('Horsens'),('Vejle'),('Roskilde');

-- ─────────────────────────────────────────────────────────────
-- Trainers (25)
-- ─────────────────────────────────────────────────────────────
INSERT INTO trainer (Name) VALUES
('Mads Jensen'),('Sofie Andersen'),('Lucas Christensen'),('Emma Nielsen'),('Noah Petersen'),
('Ida Thomsen'),('Oliver Møller'),('Astrid Larsen'),('William Hansen'),('Clara Pedersen'),
('Magnus Rasmussen'),('Freja Sørensen'),('Elias Eriksen'),('Nora Madsen'),('Victor Dahl'),
('Maja Kristiansen'),('Sebastian Koch'),('Anna Holm'),('Marcus Lund'),('Isabella Bech'),
('Tobias Nygaard'),('Cecilie Hvid'),('Emil Bruun'),('Laura Stein'),('Christian Fog');

-- ─────────────────────────────────────────────────────────────
-- Centers (10)
-- ─────────────────────────────────────────────────────────────
INSERT INTO Center (LocationID) VALUES
(1),(1),(2),(2),(3),(4),(5),(6),(7),(8);

-- ─────────────────────────────────────────────────────────────
-- Halls (20)
-- ─────────────────────────────────────────────────────────────
INSERT INTO Hall (CenterID, Name) VALUES
(1,'Cardio Hall'),(1,'Weight Room'),(2,'Yoga Studio'),(2,'Spin Room'),
(3,'Boxing Ring'),(3,'Dance Floor'),(4,'Pilates Room'),(4,'Aqua Hall'),
(5,'Crossfit Zone'),(5,'Recovery Room'),(6,'Stretching Area'),(6,'Heavy Lift Room'),
(7,'Cycling Studio'),(7,'Group Class Hall'),(8,'Martial Arts Room'),(8,'Rehab Room'),
(9,'Main Floor'),(9,'Cardio Deck'),(10,'Bootcamp Hall'),(10,'Functional Zone');

-- ─────────────────────────────────────────────────────────────
-- Equipment (30)
-- ─────────────────────────────────────────────────────────────
INSERT INTO Equipment (Name, CenterID) VALUES
('Treadmill',1),('Rowing Machine',1),('Stationary Bike',1),('Dumbbells Set',1),('Barbell Set',1),
('Pull-up Bar',2),('Kettlebells',2),('Battle Ropes',2),('Medicine Balls',2),('Foam Rollers',2),
('Leg Press',3),('Cable Machine',3),('Smith Machine',3),('Bench Press',3),('TRX Suspension',3),
('Elliptical',4),('Stair Climber',4),('Jump Rope',4),('Resistance Bands',4),('Yoga Mats',4),
('Punching Bags',5),('Speed Bag',5),('Agility Ladder',5),('Plyometric Box',5),('Squat Rack',5),
('Leg Curl Machine',6),('Chest Fly Machine',6),('Shoulder Press Machine',6),('Lat Pulldown',6),('Hip Abductor',6);

-- ─────────────────────────────────────────────────────────────
-- Subscriptions (5)
-- ─────────────────────────────────────────────────────────────
INSERT INTO subscription (Type, Price) VALUES
('Basic Monthly',199.00),
('Premium Monthly',349.00),
('Annual Basic',1599.00),
('Annual Premium',2999.00),
('Student',149.00);

-- ─────────────────────────────────────────────────────────────
-- Members (110)
-- ─────────────────────────────────────────────────────────────
INSERT INTO member (Name, Email, TrainerID, BirthDate) VALUES
('Alexander Berg','alexander.berg@mail.dk',1,'1990-03-15'),
('Simone Dall','simone.dall@mail.dk',1,'1995-07-22'),
('Patrick Holm','patrick.holm@mail.dk',2,'1988-11-30'),
('Katrine Voss','katrine.voss@mail.dk',2,'1993-04-18'),
('Frederik Møller','frederik.moller@mail.dk',3,'1997-09-05'),
('Julie Frost','julie.frost@mail.dk',3,'1991-12-14'),
('Mikkel Dahl','mikkel.dahl@mail.dk',4,'1985-06-28'),
('Natasja Kirk','natasja.kirk@mail.dk',4,'1999-02-09'),
('Rasmus Bay','rasmus.bay@mail.dk',5,'1994-08-17'),
('Camilla Winge','camilla.winge@mail.dk',5,'1987-05-03'),
('Andreas Elg','andreas.elg@mail.dk',6,'1996-01-25'),
('Stine Munk','stine.munk@mail.dk',6,'1992-10-11'),
('Jonas Knudsen','jonas.knudsen@mail.dk',7,'1989-07-07'),
('Christina Pagh','christina.pagh@mail.dk',7,'1998-03-19'),
('Lars Frost','lars.frost@mail.dk',8,'1986-11-08'),
('Maria Lund','maria.lund@mail.dk',8,'2000-05-30'),
('Thomas Eg','thomas.eg@mail.dk',9,'1993-09-23'),
('Sara Brix','sara.brix@mail.dk',9,'1990-01-16'),
('Daniel Vang','daniel.vang@mail.dk',10,'1997-04-04'),
('Louise Kirk','louise.kirk@mail.dk',10,'1988-08-12'),
('Morten Vig','morten.vig@mail.dk',11,'1995-06-20'),
('Rikke Dall','rikke.dall@mail.dk',11,'1991-12-01'),
('Peter Sand','peter.sand@mail.dk',12,'1984-03-27'),
('Hanne Fog','hanne.fog@mail.dk',12,'1999-10-08'),
('Klaus Bruun','klaus.bruun@mail.dk',13,'1986-07-14'),
('Tine Hjort','tine.hjort@mail.dk',13,'1994-02-22'),
('Jesper Ravn','jesper.ravn@mail.dk',14,'1990-09-09'),
('Birgitte Sol','birgitte.sol@mail.dk',14,'1987-05-17'),
('Henrik Feld','henrik.feld@mail.dk',15,'1993-11-28'),
('Dorte Sig','dorte.sig@mail.dk',15,'1996-04-03'),
('Sune Borg','sune.borg@mail.dk',16,'1998-08-25'),
('Pernille Fly','pernille.fly@mail.dk',16,'1985-01-13'),
('Niels Krog','niels.krog@mail.dk',17,'1991-06-06'),
('Anne Elg','anne.elg@mail.dk',17,'2001-03-31'),
('Bo Hjul','bo.hjul@mail.dk',18,'1988-10-19'),
('Vibeke Skov','vibeke.skov@mail.dk',18,'1994-07-07'),
('Finn Holt','finn.holt@mail.dk',19,'1983-04-14'),
('Gitte Lyk','gitte.lyk@mail.dk',19,'1997-12-25'),
('Stig Rud','stig.rud@mail.dk',20,'1990-08-08'),
('Bente Alm','bente.alm@mail.dk',20,'1986-02-18'),
('Karsten Vik','karsten.vik@mail.dk',21,'1992-05-05'),
('Lone Berg','lone.berg@mail.dk',21,'1995-09-14'),
('Preben Hoj','preben.hoj@mail.dk',22,'1984-01-20'),
('Inge Skov','inge.skov@mail.dk',22,'1999-07-03'),
('Bent Falk','bent.falk@mail.dk',23,'1987-11-11'),
('Ulla Ped','ulla.ped@mail.dk',23,'1993-03-24'),
('Aage Elk','aage.elk@mail.dk',24,'1980-06-16'),
('Ruth Jens','ruth.jens@mail.dk',24,'1996-10-07'),
('Svend Rask','svend.rask@mail.dk',25,'1988-04-29'),
('Grethe Ly','grethe.ly@mail.dk',25,'1991-08-22'),
('Anders Poul','anders.poul@mail.dk',1,'1994-12-10'),
('Birte Holm','birte.holm@mail.dk',2,'2000-03-05'),
('Carl Brun','carl.brun@mail.dk',3,'1989-07-17'),
('Ditte Vest','ditte.vest@mail.dk',4,'1997-01-08'),
('Erik Mand','erik.mand@mail.dk',5,'1983-05-21'),
('Frida Bak','frida.bak@mail.dk',6,'1998-09-13'),
('Georg Sand','georg.sand@mail.dk',7,'1992-02-28'),
('Helle Toft','helle.toft@mail.dk',8,'1986-06-04'),
('Ivan Ris','ivan.ris@mail.dk',9,'1995-11-16'),
('Jette Dal','jette.dal@mail.dk',10,'1990-04-09'),
('Kim Vest','kim.vest@mail.dk',11,'1985-08-23'),
('Lotte Bak','lotte.bak@mail.dk',12,'1999-01-30'),
('Mogens Fyn','mogens.fyn@mail.dk',13,'1987-05-12'),
('Nette Bak','nette.bak@mail.dk',14,'1993-10-25'),
('Ole Juel','ole.juel@mail.dk',15,'1982-03-07'),
('Pia Krag','pia.krag@mail.dk',16,'1997-07-19'),
('Qui Vu','qui.vu@mail.dk',17,'1991-12-02'),
('Rita Kold','rita.kold@mail.dk',18,'1988-04-14'),
('Søren Vest','soren.vest@mail.dk',19,'1996-08-27'),
('Tove Mark','tove.mark@mail.dk',20,'1984-02-09'),
('Uffe Leth','uffe.leth@mail.dk',21,'1993-06-22'),
('Vera Pind','vera.pind@mail.dk',22,'1989-10-05'),
('Werner Sol','werner.sol@mail.dk',23,'1995-03-18'),
('Xenia Brun','xenia.brun@mail.dk',24,'2000-07-31'),
('Yusuf Ali','yusuf.ali@mail.dk',25,'1992-01-13'),
('Zara Poul','zara.poul@mail.dk',1,'1987-05-26'),
('Aksel Hav','aksel.hav@mail.dk',2,'1994-09-08'),
('Bea Nors','bea.nors@mail.dk',3,'1999-02-21'),
('Cato Bak','cato.bak@mail.dk',4,'1986-06-14'),
('Dea Falk','dea.falk@mail.dk',5,'1993-10-27'),
('Emil Lyk','emil.lyk@mail.dk',6,'1990-04-09'),
('Fiona Dal','fiona.dal@mail.dk',7,'1997-08-22'),
('Gorm Vest','gorm.vest@mail.dk',8,'1984-01-04'),
('Hilda Ny','hilda.ny@mail.dk',9,'1998-05-17'),
('Ivar Berg','ivar.berg@mail.dk',10,'1991-09-30'),
('Jana Kold','jana.kold@mail.dk',11,'1988-03-12'),
('Kent Ravn','kent.ravn@mail.dk',12,'1995-07-25'),
('Lena Skov','lena.skov@mail.dk',13,'2001-01-07'),
('Max Frost','max.frost@mail.dk',14,'1985-05-20'),
('Nina Brix','nina.brix@mail.dk',15,'1992-09-02'),
('Oscar Vig','oscar.vig@mail.dk',16,'1989-02-15'),
('Petra Sol','petra.sol@mail.dk',17,'1996-06-28'),
('Ralf Juel','ralf.juel@mail.dk',18,'1983-10-10'),
('Sif Hjort','sif.hjort@mail.dk',19,'1997-03-23'),
('Tomas Bak','tomas.bak@mail.dk',20,'1994-07-06'),
('Ulla Fyn','ulla.fyn@mail.dk',21,'1990-11-18'),
('Viggo Mand','viggo.mand@mail.dk',22,'1987-04-01'),
('Winnie Elk','winnie.elk@mail.dk',23,'1993-08-14'),
('Xavier Park','xavier.park@mail.dk',24,'1985-12-27'),
('Ylva Borg','ylva.borg@mail.dk',25,'1999-05-09'),
('Zoe Knud','zoe.knud@mail.dk',1,'1996-09-22'),
('Adam Lund','adam.lund@mail.dk',2,'1991-02-04'),
('Bella Rud','bella.rud@mail.dk',3,'1988-06-17'),
('Casper Stig','casper.stig@mail.dk',4,'1995-10-30'),
('Diana Sol','diana.sol@mail.dk',5,'2000-03-13');

-- ─────────────────────────────────────────────────────────────
-- Classes (110)
-- ─────────────────────────────────────────────────────────────
INSERT INTO class (Name, TrainerID, ClassDate, HallID, LocationID) VALUES
('Morning Yoga',1,'2025-06-02 07:00:00',3,1),
('Spin Blast',2,'2025-06-02 09:00:00',4,1),
('CrossFit WOD',3,'2025-06-02 10:30:00',9,2),
('Boxing Fundamentals',4,'2025-06-03 08:00:00',5,3),
('Pilates Core',5,'2025-06-03 09:30:00',7,4),
('Zumba Fever',6,'2025-06-03 11:00:00',6,1),
('HIIT Circuit',7,'2025-06-04 06:30:00',1,1),
('Aqua Aerobics',8,'2025-06-04 10:00:00',8,2),
('Bootcamp Elite',9,'2025-06-04 12:00:00',19,5),
('Stretch & Recover',10,'2025-06-05 08:00:00',11,6),
('Power Lifting',11,'2025-06-05 10:00:00',12,6),
('Cycling Sprint',12,'2025-06-05 17:30:00',13,7),
('Dance Cardio',13,'2025-06-06 09:00:00',14,7),
('Martial Arts Basics',14,'2025-06-06 10:30:00',15,8),
('Rehab Mobility',15,'2025-06-06 11:00:00',16,8),
('Functional Training',16,'2025-06-07 07:00:00',20,9),
('Evening Yoga',17,'2025-06-07 18:00:00',3,1),
('Tabata Burn',18,'2025-06-07 09:00:00',1,1),
('Yin Yoga',19,'2025-06-08 10:00:00',3,1),
('Strongman Training',20,'2025-06-08 11:00:00',12,6),
('Jump Rope Cardio',21,'2025-06-09 07:30:00',1,1),
('Kettlebell Flow',22,'2025-06-09 09:00:00',9,2),
('Foam Rolling',23,'2025-06-09 10:30:00',10,2),
('Core Blast',24,'2025-06-10 06:00:00',1,1),
('Senior Fitness',25,'2025-06-10 10:00:00',7,4),
('Prenatal Yoga',1,'2025-06-10 11:30:00',3,1),
('Boxing Cardio',2,'2025-06-11 08:00:00',5,3),
('Spin Endurance',3,'2025-06-11 09:30:00',4,1),
('TRX Suspension',4,'2025-06-11 11:00:00',9,2),
('Barre Fitness',5,'2025-06-12 09:00:00',6,1),
('Kickboxing',6,'2025-06-12 10:30:00',5,3),
('Yoga Flow',7,'2025-06-12 12:00:00',3,1),
('Power Yoga',8,'2025-06-13 07:00:00',3,1),
('Metabolic Conditioning',9,'2025-06-13 09:00:00',9,2),
('Rowing Circuit',10,'2025-06-13 10:30:00',1,1),
('Cycling Intervals',11,'2025-06-14 08:00:00',13,7),
('Body Pump',12,'2025-06-14 09:30:00',14,7),
('Mindful Yoga',13,'2025-06-14 11:00:00',3,1),
('Sports Conditioning',14,'2025-06-15 07:00:00',9,2),
('Abs & Back',15,'2025-06-15 09:00:00',1,1),
('Flexibility Flow',16,'2025-06-15 10:30:00',11,6),
('Resistance Training',17,'2025-06-16 08:00:00',12,6),
('Step Aerobics',18,'2025-06-16 09:30:00',14,7),
('Vinyasa Yoga',19,'2025-06-16 11:00:00',3,1),
('Speed & Agility',20,'2025-06-17 07:30:00',9,2),
('Upper Body Sculpt',21,'2025-06-17 09:00:00',12,6),
('Lower Body Burn',22,'2025-06-17 10:30:00',1,1),
('Meditation & Breath',23,'2025-06-18 08:00:00',11,6),
('Olympic Lifting',24,'2025-06-18 10:00:00',12,6),
('Cardio Dance',25,'2025-06-18 11:30:00',6,1),
('Full Body HIIT',1,'2025-06-19 07:00:00',1,1),
('Evening Spin',2,'2025-06-19 18:00:00',4,1),
('Yoga for Athletes',3,'2025-06-19 10:00:00',3,1),
('Boxing Advanced',4,'2025-06-20 09:00:00',5,3),
('Bootcamp Beginner',5,'2025-06-20 10:30:00',19,5),
('Functional HIIT',6,'2025-06-20 12:00:00',20,9),
('Gentle Yoga',7,'2025-06-21 09:00:00',3,1),
('Sprint Intervals',8,'2025-06-21 10:00:00',9,2),
('Body Combat',9,'2025-06-21 11:30:00',15,8),
('Stability Ball',10,'2025-06-22 08:00:00',7,4),
('CrossFit Beginners',11,'2025-06-22 09:30:00',9,2),
('Hip Hop Dance',12,'2025-06-22 11:00:00',6,1),
('Balance & Coordination',13,'2025-06-23 09:00:00',11,6),
('Endurance Ride',14,'2025-06-23 10:30:00',13,7),
('Yoga Nidra',15,'2025-06-23 12:00:00',3,1),
('Athletic Performance',16,'2025-06-24 07:00:00',9,2),
('Cardio Kickboxing',17,'2025-06-24 09:00:00',5,3),
('Pilates Advanced',18,'2025-06-24 10:30:00',7,4),
('Morning HIIT',19,'2025-06-25 06:30:00',1,1),
('Restorative Yoga',20,'2025-06-25 10:00:00',3,1),
('Indoor Rowing',21,'2025-06-25 11:30:00',1,1),
('Combat Training',22,'2025-06-26 08:00:00',15,8),
('Muscle Endurance',23,'2025-06-26 09:30:00',12,6),
('Dance Fitness',24,'2025-06-26 11:00:00',6,1),
('Aqua Fit',25,'2025-06-27 09:00:00',8,2),
('Yoga & Mindfulness',1,'2025-06-27 10:30:00',3,1),
('Interval Cycling',2,'2025-06-27 12:00:00',4,1),
('Core & Flexibility',3,'2025-06-28 08:00:00',1,1),
('Parkour Basics',4,'2025-06-28 09:30:00',9,2),
('Pilates Reformer',5,'2025-06-28 11:00:00',7,4),
('Latin Dance Fitness',6,'2025-06-29 09:00:00',6,1),
('Powerbuilding',7,'2025-06-29 10:30:00',12,6),
('Foam Roll & Stretch',8,'2025-06-29 12:00:00',11,6),
('Triathlon Training',9,'2025-06-30 07:00:00',9,2),
('Sunday Yoga',10,'2025-06-30 10:00:00',3,1),
('Boxing Conditioning',11,'2025-06-30 11:30:00',5,3),
('Barbell Club',12,'2025-07-01 08:00:00',12,6),
('Hatha Yoga',13,'2025-07-01 09:30:00',3,1),
('Military Fitness',14,'2025-07-01 11:00:00',19,5),
('Cycling Fit',15,'2025-07-02 07:30:00',13,7),
('Yoga Detox',16,'2025-07-02 09:00:00',3,1),
('Power HIIT',17,'2025-07-02 10:30:00',1,1),
('Plyometrics',18,'2025-07-03 08:00:00',9,2),
('Evening Yoga Flow',19,'2025-07-03 18:00:00',3,1),
('Suspension Training',20,'2025-07-03 10:00:00',9,2),
('Cardio Boxing',21,'2025-07-04 09:00:00',5,3),
('Legs & Glutes',22,'2025-07-04 10:30:00',1,1),
('Morning Meditation',23,'2025-07-04 07:00:00',11,6),
('Olympic Prep',24,'2025-07-05 09:00:00',12,6),
('Weekend Bootcamp',25,'2025-07-05 10:30:00',19,5),
('Ashtanga Yoga',1,'2025-07-05 12:00:00',3,1),
('CrossFit Games Prep',2,'2025-07-06 08:00:00',9,2),
('Dance Party Cardio',3,'2025-07-06 10:00:00',6,1);

-- ─────────────────────────────────────────────────────────────
-- Memberships (105)
-- ─────────────────────────────────────────────────────────────
INSERT INTO membership (MemberID, SubscriptionID, StartDate)
SELECT
    m.MemberID,
    ((m.MemberID % 5) + 1),
    DATE_SUB(CURDATE(), INTERVAL (m.MemberID * 13 % 365) DAY)
FROM member m
LIMIT 105;

-- ─────────────────────────────────────────────────────────────
-- ClassBookings (150)
-- ─────────────────────────────────────────────────────────────
INSERT INTO classbooking (MemberID, ClassID)
SELECT DISTINCT
    ((seq - 1) % 105 + 1)  AS MemberID,
    ((seq - 1) % 110 + 1)  AS ClassID
FROM (
    SELECT @row := @row + 1 AS seq
    FROM information_schema.columns c1, information_schema.columns c2, (SELECT @row := 0) r
    LIMIT 200
) seq_table
WHERE ((seq - 1) % 105 + 1) <= 105
  AND ((seq - 1) % 110 + 1) <= 110
ON DUPLICATE KEY UPDATE BookingID = BookingID;

-- ─────────────────────────────────────────────────────────────
-- Payments (150)
-- ─────────────────────────────────────────────────────────────
INSERT INTO Payment (MemberID, Amount, PaymentDate, PaymentType)
SELECT
    m.MemberID,
    ROUND(149 + (m.MemberID * 37.5 % 300), 2),
    DATE_SUB(NOW(), INTERVAL (m.MemberID * 7 % 365) DAY),
    CASE (m.MemberID % 3)
        WHEN 0 THEN 'Credit Card'
        WHEN 1 THEN 'MobilePay'
        ELSE 'Cash'
    END
FROM member m
LIMIT 105;

-- Second round of payments
INSERT INTO Payment (MemberID, Amount, PaymentDate, PaymentType)
SELECT
    m.MemberID,
    ROUND(199 + (m.MemberID * 43 % 250), 2),
    DATE_SUB(NOW(), INTERVAL (m.MemberID * 11 % 300 + 30) DAY),
    CASE (m.MemberID % 3)
        WHEN 0 THEN 'MobilePay'
        WHEN 1 THEN 'Cash'
        ELSE 'Credit Card'
    END
FROM member m
WHERE m.MemberID <= 50;

-- ─────────────────────────────────────────────────────────────
-- VendingMachines (10)
-- ─────────────────────────────────────────────────────────────
INSERT INTO VendingMachine (Name, Location, CenterID) VALUES
('VM-CPH-1','Entrance Hall',1),('VM-CPH-2','Changing Room',2),
('VM-AAR-1','Lobby',3),       ('VM-AAR-2','Pool Area',4),
('VM-ODE-1','Main Floor',5),  ('VM-AAL-1','Reception',6),
('VM-ESB-1','Lounge',7),      ('VM-RAN-1','Corridor',8),
('VM-KOL-1','Break Room',9),  ('VM-HOR-1','Gym Floor',10);

-- ─────────────────────────────────────────────────────────────
-- VendingMachineStock (50)
-- ─────────────────────────────────────────────────────────────
INSERT INTO VendingMachineStock (VendingMachineID, ProductName, Quantity, Price) VALUES
(1,'Protein Bar',30,25.00),(1,'Energy Drink',20,35.00),(1,'Water 0.5L',50,15.00),(1,'Banana',15,10.00),(1,'Protein Shake',25,45.00),
(2,'Protein Bar',25,25.00),(2,'Sports Gel',40,20.00),(2,'Nuts Mix',20,30.00),(2,'Water 0.5L',45,15.00),(2,'BCAA Drink',15,40.00),
(3,'Energy Bar',35,22.00),(3,'Energy Drink',18,35.00),(3,'Apple',20,8.00),(3,'Protein Shake',10,45.00),(3,'Water 1L',30,20.00),
(4,'Protein Bar',28,25.00),(4,'Sports Drink',22,30.00),(4,'Banana',12,10.00),(4,'Creatine Shot',8,50.00),(4,'Water 0.5L',40,15.00),
(5,'Energy Bar',30,22.00),(5,'Protein Shake',15,45.00),(5,'Nuts Mix',25,30.00),(5,'Energy Drink',20,35.00),(5,'Water 0.5L',50,15.00),
(6,'Protein Bar',20,25.00),(6,'Sports Gel',35,20.00),(6,'Apple',18,8.00),(6,'BCAA Drink',10,40.00),(6,'Water 1L',25,20.00),
(7,'Energy Drink',22,35.00),(7,'Protein Bar',30,25.00),(7,'Banana',14,10.00),(7,'Water 0.5L',45,15.00),(7,'Protein Shake',12,45.00),
(8,'Sports Drink',28,30.00),(8,'Nuts Mix',20,30.00),(8,'Energy Bar',25,22.00),(8,'Water 0.5L',40,15.00),(8,'Sports Gel',30,20.00),
(9,'Protein Bar',18,25.00),(9,'Energy Drink',15,35.00),(9,'Apple',22,8.00),(9,'Water 1L',30,20.00),(9,'BCAA Drink',8,40.00),
(10,'Protein Shake',20,45.00),(10,'Sports Gel',25,20.00),(10,'Banana',16,10.00),(10,'Energy Bar',28,22.00),(10,'Water 0.5L',50,15.00);

-- ─────────────────────────────────────────────────────────────
-- Staff (15)
-- ─────────────────────────────────────────────────────────────
INSERT INTO Staff (Name, Role) VALUES
('Søren Madsen','Manager'),('Annette Holm','Receptionist'),('Brian Voss','Cleaning Staff'),
('Dorthe Sand','Manager'),('Einar Lyk','Security'),('Fanny Berg','Receptionist'),
('Gustav Ravn','IT Support'),('Helena Kirk','Manager'),('Igor Poul','Cleaning Staff'),
('Janne Falk','Receptionist'),('Karl Stig','Security'),('Lise Frost','Manager'),
('Mikael Hjort','IT Support'),('Nanna Elg','Receptionist'),('Ove Bak','Maintenance');

-- ─────────────────────────────────────────────────────────────
-- App users (5)
-- Passwords hashed with BCrypt (all are: Admin123!)
-- ─────────────────────────────────────────────────────────────
INSERT INTO app_user (Username, PasswordHash, Enabled, Role) VALUES
('superadmin','$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',1,'Admin'),
('admin','$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',1,'Admin'),
('trainer_user','$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',1,'User'),
('readonly_user','$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',1,'User'),
('staff_user','$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy',1,'User');
