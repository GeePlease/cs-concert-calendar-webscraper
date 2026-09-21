# CS Concert Calendar Webscraper

A web application that collects concert data from selected websites through web scraping and displays upcoming events in a structured concert calendar.

The project was developed as my final project during my **Application Development – Coding** training at Schulungszentrum Fohnsdorf.

## Features

* Web scraping of concert and event data
* Structured storage of scraped concert information
* Concert overview and calendar
* Monthly calendar navigation
* Filtering and displaying upcoming events
* ASP.NET Core web interface

## Technologies

* **C#**
* **ASP.NET Core**
* **Razor Pages**
* **HTML / CSS / JavaScript**
* **MongoDB**
* **Web Scraping**
* **Git / GitHub**

## How it works

The application retrieves concert information from selected event websites using web scraping.

Relevant information such as the event name, date, venue and other available details is extracted and converted into a consistent data structure.

The collected data is stored in MongoDB and used by the ASP.NET Core application to display upcoming concerts in a clear and accessible format.

## Project Structure

The application separates the main responsibilities into different components:

* **Scraping** – retrieves and extracts concert data from external websites
* **Data processing** – converts scraped information into structured concert objects
* **Database** – stores the collected concert data in MongoDB
* **Web application** – provides the user interface and concert calendar

## Purpose

The main goal of this project was to combine several topics covered during my training in one practical application, including:

* object-oriented programming with C#
* web development with ASP.NET Core
* working with databases
* processing external data
* web scraping
* structuring a larger software project

## Status

This project was created as a final training project and may be further developed in the future.

Possible future improvements include replacing the current database solution, expanding the available data sources and improving the automation of the scraping process.
