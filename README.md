# Nemaris

## Overview

This project is a restaurant management system that integrates traditional restaurant operations with an AI-powered chatbot. The system allows staff to manage reservations, tables, orders, and billing, while guests can interact with an AI assistant to check availability or request reservations using natural language.

The backend provides the core business logic and database management. The AI layer communicates with the backend through an MCP server, allowing the language model to safely access predefined tools without direct database access.

The project is designed as a modular system composed of several services that communicate through APIs.

---


<img width="1045" height="742" alt="Screenshot 2026-03-10 at 16 05 54" src="https://github.com/user-attachments/assets/967feb0e-34bf-4ef6-89a2-9abc39310534" />


### Features

Authentication and Users

* User registration and login
* JWT authentication
* Refresh token support
* Role-based access control

Table Management

* Table creation and management
* Table capacity configuration
* Table status tracking
* Table state overview
* Table grouping support

Reservations

* Create reservations
* Update and cancel reservations
* Assign reservations to tables
* Track reservation status and schedule
* Store guest details and special requests

Menu

* Menu category management
* Menu item management
* Pricing and availability control

Orders and Billing

* Open order per table
* Assign orders to waiters
* Add menu items to orders
* Track quantities and item pricing
* Automatic order total calculation
* Order status management

Payments

* Record payments
* Support multiple payment methods
* Track payment status
* Store payment reference numbers

---

## AI Features

Table Occupancy Simulation

* Simulation of table occupancy over time
* Simulation of guest arrivals and departures
* Simulation of order creation and bill closing
* Generation of realistic restaurant activity scenarios

Operational Simulation

* Simulated table usage
* Simulated order lifecycle
* Simulated guest flow inside the restaurant

AI Chat Interaction

* Users can interact with the AI through chat
* Natural language questions about tables, reservations, and availability
* AI responses based on simulated or real system state

