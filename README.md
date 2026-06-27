# Nemaris

## Overview

This project is a restaurant management system that integrates traditional restaurant operations with an AI-powered chatbot. The system allows staff to manage reservations, tables, orders, and billing, while guests can interact with an AI assistant to check availability or request reservations using natural language.

The project is designed as a modular system composed of several services that communicate through APIs.

For local development setup see [DOCKER.md](./DOCKER.md).

---


<img width="1045" height="742" alt="Screenshot 2026-03-10 at 16 05 54" src="https://github.com/user-attachments/assets/967feb0e-34bf-4ef6-89a2-9abc39310534" />


### Features

Authentication and Users

* User registration and login
* JWT authentication
* Refresh token support
* Role-based access control

Table Management

* Capacity tracked per table
* Live table status tracking (available / reserved / seated)
* Multi-floor table state overview with walk-in seating and occupancy controls

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

* Cash payment processing from the cash register UI
* Track payment status per order
* Store payment reference numbers (auto-generated, unique)

---

## AI Features

AI Chat Interaction

* Users can interact with the AI through chat
* Natural language questions about tables, reservations, and availability

