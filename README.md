# School Database Management System

The **School Database Management System** is a web application that allows users to manage students, teachers, courses, and their relationships. The system provides functionality for adding, updating, and deleting records, as well as viewing the associations between students, teachers, and courses. 

> Find the school database sql on the root folder as pdf.

## Table of Contents

- [Features](#features)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [Usage](#usage)
- [API Endpoints](#api-endpoints)
- [Contributing](#contributing)


---

## Features

- **Students Management**: Add, update, delete, and list students.
- **Teachers Management**: Add, update, delete, and list teachers, including AJAX-based operations.
- **Courses Management**: Add, update, delete, and list courses.
- **Relationships**:
  - View all courses a student is enrolled in.
  - View all students enrolled in a course.
- **Search Functionality**:
  - Search teachers by name or hire date range.

---

## Technologies Used

- **Backend**:
  - ASP.NET Core MVC
  - MySQL
- **Frontend**:
  - HTML/CSS
  - JavaScript (AJAX)
  - Bootstrap
- **Tools**:
  - Visual Studio
  - MySQL Workbench

---

## Installation

### Prerequisites

- .NET 6.0 or higher installed.
- MySQL installed and configured.

### Steps

1. Clone the repository:
   ```bash
   git clone https://github.com/your-repository-url.git
   cd SchoolDatabaseCTwo


2. Set up the database:

- Import the provided SQL schema and data into your MySQL database.

- Update the SchoolDbContext connection string in appsettings.json to match your MySQL configuration.


3. Run the application:

`dotnet run`


4. Open your browser and navigate to:

`http://localhost:<port>`


## Usage

### Teacher Management

- Add, update, or delete teacher records.

- Use AJAX to add or update teachers with dynamic error handling.

- Search for teachers by name or hire date range.

### Student Management

- Add, update, or delete student records.

- View courses a student is enrolled in.

### Course Management

- Add, update, or delete course records.

- View students enrolled in a course.


## API Endpoints

### Student API

|Method |	Endpoint	| Description |
| ---- | --------- | ----------- |
| GET  |	/api/StudentAPI/ListStudents |	Retrieve a list of all students. |
| GET	  | /api/StudentAPI/FindStudent/{id}	| Retrieve details of a specific student. |
| POST	| /api/StudentAPI/AddStudent	| Add a new student. |
| PUT  |	/api/StudentAPI/UpdateStudent	| Update an existing student. |
| POST	| /api/StudentAPI/DeleteStudent/{id}	| Delete a student. |

### Teacher API

| Method	| Endpoint	| Description |
| ------- | --------- | ----------- |
| GET	| /api/TeacherAPI/ListTeachers	| Retrieve a list of all teachers. |
| GET	| /api/TeacherAPI/FindTeacher/{id}	| Retrieve details of a specific teacher. |
| POST	| /api/TeacherAPI/AddTeacher	| Add a new teacher. |
| POST	| /api/TeacherAPI/DeleteTeacher/{id}	| Delete a teacher. |
| POST	| /api/TeacherAPI/UpdateTeacher	| Update a teacher. |

### Course API

| Method	| Endpoint	| Description |
| ------- | --------- | ----------- |
| GET	| /api/CourseAPI/ListCourses	| Retrieve a list of all courses. |
| GET	| /api/CourseAPI/FindCourse/{id}	| Retrieve details of a specific course. |
| POST	| /api/CourseAPI/AddCourse	| Add a new course. |
| POST	| /api/CourseAPI/DeleteCourse/{id}	| Delete a course. |
| PUT	 | /api/CourseAPI/UpdateCourse	 | Update an existing course. |
| GET	 | /api/CourseAPI/GetStudentsByCourse/{courseId}	| Get students enrolled in a course. |


## Fork the repository.

1. Create a feature branch:

`git checkout -b feature-name`

2. Commit your changes:

`git commit -am "Feature description"`

3. Push to your branch:

`git push origin feature-name`

4. Open a pull request.