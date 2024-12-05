function highlightField(field, errorMessage) {
    field.style.background = "red";
    field.focus();
  
    // Display error message
    var errorContainer = field.parentElement.querySelector('.error-message');
    if (errorContainer) {
      errorContainer.innerHTML = errorMessage;
    }
}


  
function validateTeacherData() {
    var teacherFname = document.getElementById("TeacherFname");
    var teacherLname = document.getElementById("TeacherLname");
    var employeeNumber = document.getElementById("EmployeeNumber");
    var employeeNumberRegEx = /(T)\d{3}$/;
    var hireDate = document.getElementById("HireDate");
    var salary = document.getElementById("Salary");
    var teacherWorkPhone = document.getElementById("TeacherWorkPhone");
    var allFields = document.querySelectorAll('.form-control');
  
    // Reset validation
    allFields.forEach(function (element) {
      element.style.background = ""; 
    });
  
    // Reset validation messages
    var errorMessages = document.querySelectorAll('.text-danger');
    errorMessages.forEach(function (element) {
      element.innerHTML = ""; // Clear existing error messages
    });
  
    // Validate fields
    if (teacherFname.value === "") {
      highlightField(teacherFname, "First name is required");
      return false;
    }
    if (teacherLname.value === "") {
      highlightField(teacherLname, "Last name is required");
      return false;
    }
    if (employeeNumber.value === "" || !employeeNumberRegEx.test(employeeNumber.value)) {
      highlightField(employeeNumber, "Employee number must start with 'T' followed by 3 digits");
      return false;
    }
    if (hireDate.value === "" || new Date(hireDate.value) > new Date()) {
      highlightField(hireDate, "Hire date cannot be empty and must be in the past");
      return false;
    }
    if (salary.value === "" || salary.value < 0) {
      highlightField(salary, "Salary cannot be empty and must be a positive value");
      return false;
    }
    if (teacherWorkPhone.value === "") { 
      highlightField(teacherWorkPhone, "Work phone is required");
      return false;
    }
  
    return true;
}
  
  

function AddTeacher() {
    var teacherFname = document.getElementById("TeacherFname");
    var teacherLname = document.getElementById("TeacherLname");
    var employeeNumber = document.getElementById("EmployeeNumber");
    var hireDate = document.getElementById("HireDate");
    var salary = document.getElementById("Salary");
    var teacherWorkPhone = document.getElementById("TeacherWorkPhone");
  
    if (!validateTeacherData()) {
      return false;
    }
  
    var teacherData = {
      "TeacherFname": teacherFname.value,
      "TeacherLname": teacherLname.value,
      "EmployeeNumber": employeeNumber.value,
      "HireDate": hireDate.value,
      "Salary": salary.value,
      "TeacherWorkPhone": teacherWorkPhone.value
    };
  
    /// Send data to the server
    var URL = "/api/TeacherAPI/AddTeacher/";
    var httpRequest = new XMLHttpRequest();
    var responseText = document.getElementById("ResponseText");
  
    httpRequest.open("POST", URL, true);
    httpRequest.setRequestHeader("Content-Type", "application/json");
    httpRequest.onreadystatechange = function () {
        if (httpRequest.readyState == 4) {
          if (httpRequest.status == 200) {
              var successResponse = JSON.parse(httpRequest.responseText);
              alert(successResponse.message);
              window.location.href = "/TeacherPage/List";
          } else {
              var errorResponse = JSON.parse(httpRequest.responseText || "{}");
              responseText.innerHTML = "Error: " + (errorResponse.message || "An error occurred.");
          }
        }
    };
  
    httpRequest.send(JSON.stringify(teacherData));
}
  
  

function addTeacherData(data) {
      var URL = "/api/TeacherAPI/AddTeacher/";
      var httpRequest = new XMLHttpRequest();
  
      var responseText = document.getElementById("ResponseText");
  
      httpRequest.open("POST", URL, true);
      httpRequest.setRequestHeader("Content-Type", "application/json");
      httpRequest.onreadystatechange = function () {
          if (httpRequest.readyState == 4) {
              if (httpRequest.status == 200) {
                  // Success response
                  var successResponse = JSON.parse(httpRequest.responseText);
                  alert(successResponse.message);
                  window.location.href = "/TeacherPage/List";
              } else if (httpRequest.status == 400) {
                  // Bad request response 
                  var errorResponse = JSON.parse(httpRequest.responseText);
                  responseText.innerHTML = errorResponse.message || "An error occurred.";
              } else {
                  // Other errors response
                  responseText.innerHTML = "Error: Failed to add teacher. Status: " + httpRequest.status;
                }
            }
        };
  
    httpRequest.send(JSON.stringify(data));
}
  
  

function DeleteTeacher(TeacherId) {
    var data = { "id": TeacherId };
    // AJAX request to send teacher data to server
    var URL = "/api/TeacherAPI/DeleteTeacher/" + TeacherId;
    var httpRequest = new XMLHttpRequest();
  
    var responseText = document.getElementById("ResponseText");
  
    httpRequest.open("POST", URL, true);
    httpRequest.setRequestHeader("Content-Type", "application/json");
    httpRequest.onreadystatechange = function () {
        if (httpRequest.readyState == 4) {
            if (httpRequest.status == 200) {
                // Success response
                window.location.href = "/TeacherPage/List";
            } else {
                // Other error response
                responseText.innerHTML = "Error: Failed to Delete teacher. Status: " + httpRequest.status;
            }
        }
    };
    httpRequest.send(JSON.stringify(data));
}
  
  


function UpdateTeacher(TeacherId) {
    var teacherFname = document.getElementById("TeacherFname");
    var teacherLname = document.getElementById("TeacherLname");
    var employeeNumber = document.getElementById("EmployeeNumber");
    var hireDate = document.getElementById("HireDate");
    var salary = document.getElementById("Salary");
    var teacherWorkPhone = document.getElementById("TeacherWorkPhone");
  
    if (!validateTeacherData()) {
      return false;
    }
  
    var teacherData = {
        "TeacherId": TeacherId,
        "TeacherFname": teacherFname.value,
        "TeacherLname": teacherLname.value,
        "EmployeeNumber": employeeNumber.value,
        "HireDate": hireDate.value,
        "Salary": salary.value,
        "TeacherWorkPhone": teacherWorkPhone.value 
    };
  
    // Send data to server
    updateTeacherData(teacherData);
  
    return false;
}
  
  


function updateTeacherData(data) {
    console.log("Data being sent:", data);
    // AJAX request to send teacher data to server
    var URL = "/api/TeacherAPI/" + data.TeacherId;
    var httpRequest = new XMLHttpRequest();
  
    var responseText = document.getElementById("ResponseText");
  
    httpRequest.open("POST", URL, true);
    httpRequest.setRequestHeader("Content-Type", "application/json");
    httpRequest.onreadystatechange = function () {
        if (httpRequest.readyState == 4) {
            if (httpRequest.status == 200) {
                // Success response
                window.location.href = "/TeacherPage/show/" + data.TeacherId;
            } else if (httpRequest.status == 400) {
                // Bad request response (invalid data)
                const errorResponse = JSON.parse(httpRequest.response || "{}");
                responseText.innerHTML = "Error: " + (errorResponse.message || "Invalid input data.");
            } else {
                // Other error response 
                responseText.innerHTML = "Error: Failed to update teacher. Status: " + httpRequest.status;
            }
        }
    };
  
    try {
        httpRequest.send(JSON.stringify(data));
    } catch (error) {
        console.error("Error stringifying data:", error);
        responseText.innerHTML = "Error: Unable to process the request.";
    }
}
  