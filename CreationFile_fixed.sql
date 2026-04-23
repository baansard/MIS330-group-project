create table employee(
employeeid int not null,
employeefirst varchar(25) not null,
employeelast varchar(25) not null,
employeeemail varchar(30) not null,
employeeuser varchar(16) not null,
employeepassword varchar(32) not null,
primary key (employeeid),
unique(employeeuser),
unique(employeeemail)
);

create table employeephone(
employeephone varchar(12) null,
empid int not null,
foreign key (empid) references employee(employeeid),
primary key (empid)
);

create table customer(
customerid int not null,
custfirstname varchar(25) not null,
custlastname varchar(25) not null,
custemail varchar(30) null,
custusername varchar(16) not null,
custpassword varchar(16) not null,
custphone varchar(12) null,
primary key(customerid),
unique(custemail),
unique(custusername)
);

create table trip(
tripid int not null,
tripname varchar(25) not null,
tripdate date not null,
tripstatus varchar(10) not null,
starttime time not null,
tripdescription varchar(256) null,
maxcapacity int not null,
distancemiles double not null,
lengthhours int not null,
tripprice double not null,
street varchar(60) null,
city varchar(15) not null,
state varchar(2) not null,
zip varchar(5) not null,
empid int not null,
primary key (tripid),
foreign key (empid) references employee(employeeid)
);

create table reservation(
reservationid int not null,
spotsreserved int null,
reservationdate date not null,
reservationstatus varchar(15) not null,
customerid int not null,
primary key (reservationid),
foreign key (customerid) references customer(customerid)
);

create table reservationtrip(
reservationid int not null,
tripid int not null,
primary key (reservationid, tripid),
foreign key (reservationid) references reservation(reservationid),
foreign key (tripid) references trip(tripid)
);

-- add into employee table
insert into employee(employeeid, employeefirst, employeelast, employeeemail, employeeuser, employeepassword)
values(11111, "Jane", "Smith", "JaneSmith1@gmail.com", "JaneSmith1", "Buttercup");
insert into employee(employeeid, employeefirst, employeelast, employeeemail, employeeuser, employeepassword)
values(11112, "Daphne", "Johnson", "DJ@gmail.com", "DJohnson", "employeePassword");
insert into employee(employeeid, employeefirst, employeelast, employeeemail, employeeuser, employeepassword)
values(11113, "Joe", "Johnson", "JJohnson@hotmail.com", "JJohnson1", "Password");
insert into employee(employeeid, employeefirst, employeelast, employeeemail, employeeuser, employeepassword)
values(11114, "Fred", "Jones", "Freddy1@aol.com", "Freddy1Jones", "Scooby");

-- add into employeephone table
insert into employeephone(empid, employeephone)
values(11111, "555-444-3333");
insert into employeephone(empid, employeephone)
values(11112, "555-888-9999");
insert into employeephone(empid, employeephone)
values(11113, "555-777-8888");
insert into employeephone(empid, employeephone)
values(11114, "555-666-7777");

-- add into customer table
insert into customer(customerid, custfirstname, custlastname, custemail, custusername, custpassword, custphone)
values(11111, "Bob", "Jones", "email@email.com", "username1", "password", "255-555-5555");
insert into customer(customerid, custfirstname, custlastname, custemail, custusername, custpassword, custphone)
values(11112, "John", "Smith", "email@hotmail.com", "username2", "password", "244-555-5555");
insert into customer(customerid, custfirstname, custlastname, custemail, custusername, custpassword, custphone)
values(11113, "Jane", "Doe", "email@gmail.com", "JDoe1", "password5", "555-555-5555");
insert into customer(customerid, custfirstname, custlastname, custemail, custusername, custpassword, custphone)
values(11114, "Joe", "Schmoe", "JSchmoe@aol.com", "JSchome1", "password1234", "444-555-5555");

-- add into trip table
insert into trip(tripid, tripname, tripdate, tripstatus, starttime, tripdescription, maxcapacity, distancemiles, lengthhours, tripprice, street, city, state, zip, empid)
values(11111, "Birmingham trip", "2026-07-25", "Incomplete", "8:30:00", "Day trip to Birmingham", 25, 50, 6, 550.67, "2100 Richard Arrington JR BLVD", "Birmingham", "AL", "35203", 11111);
insert into trip(tripid, tripname, tripdate, tripstatus, starttime, tripdescription, maxcapacity, distancemiles, lengthhours, tripprice, street, city, state, zip, empid)
values(11112, "Museum Trip", "2023-05-11", "Incomplete", "10:30:00", "Trip to local museums", 15, 10, 6, 65.00, "University BLVD", "Tuscaloosa", "AL", "35401", 11112);
insert into trip(tripid, tripname, tripdate, tripstatus, starttime, tripdescription, maxcapacity, distancemiles, lengthhours, tripprice, street, city, state, zip, empid)
values(11113, "Trip C", "2027-12-31", "Incomplete", "12:00:00", "A placeholder trip", 25, 0, 0, 0, "Placeholder", "Tuscaloosa", "AL", "35401", 11112);
insert into trip(tripid, tripname, tripdate, tripstatus, starttime, tripdescription, maxcapacity, distancemiles, lengthhours, tripprice, street, city, state, zip, empid)
values(11114, "Aquarium Trip", "2026-06-05", "Incomplete", "08:00:00", "Day trip to aquarium", 20, 508, 12, 225.00, "2100 E Beach BLVD", "Gulfport", "MS", "39501", 11114);

-- add into reservation table
insert into reservation(reservationid, spotsreserved, reservationdate, reservationstatus, customerid)
values(00001, 2, "2026-05-20", "Confirmed", 11111);
insert into reservation(reservationid, spotsreserved, reservationdate, reservationstatus, customerid)
values(00002, 1, "2026-05-21", "Pending", 11112);
insert into reservation(reservationid, spotsreserved, reservationdate, reservationstatus, customerid)
values(00003, 4, "2026-05-23", "Confirmed", 11111);
insert into reservation(reservationid, spotsreserved, reservationdate, reservationstatus, customerid)
values(00004, 1, "2026-05-24", "Pending", 11114);

-- add into reservationtrip table
insert into reservationtrip(reservationid, tripid)
values(00001, 11111);
insert into reservationtrip(reservationid, tripid)
values(00002, 11111);
insert into reservationtrip(reservationid, tripid)
values(00003, 11114);
insert into reservationtrip(reservationid, tripid)
values(00004, 11113);
