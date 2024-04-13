# HR Account Management Web Application

This web application, built using C#, serves as a login portal for HR Account Management. It provides functionality for normal users to login and access their account information. Additionally, administrators have privileges to view employee information and perform actions such as adding and removing employees from the XML document that stores the data.

## Purpose

The primary purpose of this project is to demonstrate the potential dangers of XPath injection and its critical implications for a company's security. By showcasing a vulnerable login portal, users can understand the risks associated with improper input validation and the importance of implementing robust security measures.

## Features

- **User Authentication**: Users can securely login to access their account information.
- **Role-based Access Control**: Administrators have elevated privileges to view and manage employee data.
- **Employee Management**: Administrators can add new employees to the system and remove existing ones.

## Vulnerability Demonstration

This project includes a demonstration of XPath injection vulnerability, allowing users to exploit weaknesses in the login portal's authentication mechanism. Through carefully crafted input, attackers can bypass authentication and gain unauthorized access to sensitive information or perform malicious actions such as modifying employee records.

## Installation

1. Clone the repository to your local machine:

   ```
   git clone https://github.com/your/repository.git
   ```

2. Open the project in your preferred C# development environment.

3. Build and run the project.

## Usage

1. Access the web application through your browser.
2. Log in as a normal user or an administrator using the provided credentials.
3. Explore the functionalities available based on your role.
4. Experiment with XPath injection by manipulating input fields to bypass authentication or access unauthorized information.

## Disclaimer

This project is for educational purposes only and should not be deployed in a production environment without appropriate security measures. Be cautious when experimenting with XPath injection and ensure that your actions comply with ethical standards and legal regulations.
``` 
