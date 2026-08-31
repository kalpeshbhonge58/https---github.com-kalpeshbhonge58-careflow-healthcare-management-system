# CareFlow API Testing Guide
## Phase 3 & 4 - API Verification

**Date**: August 31, 2026  
**API Version**: 1.0  
**Status**: Ready for Testing

---

## Quick Start: Run the API

### Step 1: Build the Solution
```bash
cd "c:\Users\kalpe\Desktop\Angular Learning Tutorials\CareFlow Healthcare Management System"
dotnet build
```

**Expected Result**: 0 Errors, 0 Warnings

### Step 2: Run the API
```bash
cd src/CareFlow.Api
dotnet run
```

**Expected Output**:
```
info: CareFlow.Api
      CareFlow Healthcare Management API started
info: Microsoft.AspNetCore.Hosting
      Listening on https://localhost:7123
```

### Step 3: Access Swagger UI
Navigate to: **https://localhost:7123/swagger**

---

## Test Users (Pre-Seeded)

Use these credentials to test different roles:

### Admin User
```
Email: admin@careflow.com
Password: Admin@123
Role: Admin
```

### Doctor User
```
Email: dr.smith@careflow.com
Password: Doctor@123
Role: Doctor
Specialization: Cardiology
```

### Receptionist User
```
Email: reception1@careflow.com
Password: Reception@123
Role: Receptionist
```

### Patient User
```
Email: john.doe@email.com
Password: Patient@123
Role: Patient
Patient Number: PAT-00001
```

---

## Testing Workflow

### Phase A: Authentication Tests

#### Test 1: Admin Login
```bash
curl -X POST https://localhost:7123/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@careflow.com",
    "password": "Admin@123"
  }'
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-08-31T15:00:00Z",
    "user": {
      "id": 1,
      "email": "admin@careflow.com",
      "firstName": "Admin",
      "lastName": "User",
      "roles": ["Admin"]
    }
  }
}
```

**Save the token** for subsequent requests.

#### Test 2: Get Current User
```bash
curl -X GET https://localhost:7123/api/auth/me \
  -H "Authorization: Bearer {TOKEN_FROM_TEST_1}"
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": 1,
    "email": "admin@careflow.com",
    "firstName": "Admin",
    "lastName": "User",
    "roles": ["Admin"],
    "lastLoginAt": "2026-08-31T14:30:00Z"
  }
}
```

#### Test 3: Invalid Credentials
```bash
curl -X POST https://localhost:7123/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@careflow.com",
    "password": "WrongPassword"
  }'
```

**Expected Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Invalid email or password.",
  "errors": []
}
```

---

### Phase B: Patient Endpoint Tests

#### Test 4: List Patients (Paginated)
```bash
curl -X GET "https://localhost:7123/api/patients?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer {ADMIN_TOKEN}"
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "email": "john.doe@email.com",
        "firstName": "John",
        "lastName": "Doe",
        "phoneNumber": "+1-555-0301",
        "patientNumber": "PAT-00001",
        "gender": "Male",
        "bloodGroup": "O+",
        "isActive": true
      },
      ...
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 5,
    "totalPages": 1
  }
}
```

#### Test 5: Get Patient Details
```bash
curl -X GET https://localhost:7123/api/patients/1 \
  -H "Authorization: Bearer {TOKEN}"
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "id": 1,
    "user": { /* user details */ },
    "patientNumber": "PAT-00001",
    "dateOfBirth": "1985-03-15",
    "gender": "Male",
    "bloodGroup": "O+",
    "address": "123 Main St, Springfield",
    "emergencyContactName": "Mary Doe",
    "emergencyContactPhone": "+1-555-0306",
    "allergies": "Penicillin",
    "totalAppointments": 3,
    "totalMedicalRecords": 1,
    "recentAppointments": [ /* appointments */ ]
  }
}
```

#### Test 6: Create New Patient
```bash
curl -X POST https://localhost:7123/api/patients \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "new.patient@email.com",
    "password": "NewPatient@123",
    "firstName": "New",
    "lastName": "Patient",
    "phoneNumber": "+1-555-9999",
    "dateOfBirth": "1990-01-01",
    "gender": "Female",
    "bloodGroup": "A+",
    "address": "456 Oak Ave",
    "emergencyContactName": "Contact Name",
    "emergencyContactPhone": "+1-555-8888"
  }'
```

**Expected Response** (201 Created):
```json
{
  "success": true,
  "message": "Patient created successfully.",
  "data": {
    "id": 11,
    "email": "new.patient@email.com",
    "firstName": "New",
    "lastName": "Patient",
    "patientNumber": "PAT-00006"
  }
}
```

---

### Phase C: Doctor Endpoint Tests

#### Test 7: List Doctors (Public)
```bash
curl -X GET "https://localhost:7123/api/doctors?pageNumber=1&pageSize=10" \
  -H "Content-Type: application/json"
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "firstName": "James",
        "lastName": "Smith",
        "phoneNumber": "+1-555-0201",
        "specialization": "Cardiology",
        "department": "Cardiology",
        "consultationFee": 250.00,
        "isActive": true
      },
      ...
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 3,
    "totalPages": 1
  }
}
```

#### Test 8: Get Active Doctors
```bash
curl -X GET https://localhost:7123/api/doctors/active
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "firstName": "James",
      "lastName": "Smith",
      "specialization": "Cardiology",
      "consultationFee": 250.00
    },
    ...
  ]
}
```

#### Test 9: Filter Doctors by Department
```bash
curl -X GET "https://localhost:7123/api/doctors?departmentId=1&pageNumber=1&pageSize=10"
```

---

### Phase D: Appointment Endpoint Tests

#### Test 10: List Appointments
```bash
curl -X GET "https://localhost:7123/api/appointments?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer {TOKEN}"
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "patientName": "John Doe",
        "doctorName": "James Smith",
        "appointmentDate": "2026-08-31",
        "startTime": "09:00:00",
        "endTime": "09:30:00",
        "status": "Scheduled",
        "reason": "Chest pain evaluation"
      },
      ...
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 10,
    "totalPages": 1
  }
}
```

#### Test 11: Book Appointment
```bash
curl -X POST https://localhost:7123/api/appointments \
  -H "Authorization: Bearer {PATIENT_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": 1,
    "doctorId": 1,
    "appointmentDate": "2026-09-15",
    "startTime": "14:00:00",
    "endTime": "14:30:00",
    "reason": "Follow-up consultation"
  }'
```

**Expected Response** (201 Created):
```json
{
  "success": true,
  "message": "Appointment booked successfully.",
  "data": {
    "id": 11,
    "patientId": 1,
    "doctorId": 1,
    "appointmentDate": "2026-09-15",
    "status": "Scheduled",
    "reason": "Follow-up consultation"
  }
}
```

#### Test 12: Confirm Appointment
```bash
curl -X PUT https://localhost:7123/api/appointments/11/confirm \
  -H "Authorization: Bearer {DOCTOR_TOKEN}" \
  -H "Content-Type: application/json"
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "message": "Appointment confirmed successfully.",
  "data": {
    "id": 11,
    "status": "Confirmed"
  }
}
```

#### Test 13: Cancel Appointment
```bash
curl -X PUT https://localhost:7123/api/appointments/1/cancel \
  -H "Authorization: Bearer {TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "cancellationReason": "Doctor emergency"
  }'
```

---

### Phase E: Medical Records Tests

#### Test 14: Create Medical Record
```bash
curl -X POST https://localhost:7123/api/medical-records \
  -H "Authorization: Bearer {DOCTOR_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": 1,
    "doctorId": 1,
    "appointmentId": 1,
    "diagnosis": "Type 2 Diabetes",
    "symptoms": "Elevated blood glucose",
    "treatment": "Prescribed Metformin",
    "followUpDate": "2026-09-30"
  }'
```

**Expected Response** (201 Created):
```json
{
  "success": true,
  "message": "Medical record created successfully.",
  "data": {
    "id": 3,
    "patientId": 1,
    "diagnosis": "Type 2 Diabetes",
    "visitDate": "2026-08-31"
  }
}
```

#### Test 15: Get Patient's Medical Records
```bash
curl -X GET https://localhost:7123/api/medical-records/patient/1 \
  -H "Authorization: Bearer {TOKEN}"
```

---

### Phase F: Prescription Tests

#### Test 16: Create Prescription
```bash
curl -X POST https://localhost:7123/api/prescriptions \
  -H "Authorization: Bearer {DOCTOR_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": 1,
    "doctorId": 1,
    "medicalRecordId": 1,
    "prescriptionDate": "2026-08-31",
    "instructions": "Take with meals",
    "items": [
      {
        "medicineId": 1,
        "dosage": "500mg",
        "frequency": "Twice daily",
        "durationDays": 14,
        "quantity": 28
      },
      {
        "medicineId": 2,
        "dosage": "10mg",
        "frequency": "Once daily",
        "durationDays": 30,
        "quantity": 30
      }
    ]
  }'
```

**Expected Response** (201 Created):
```json
{
  "success": true,
  "message": "Prescription saved successfully.",
  "data": {
    "id": 3,
    "patientId": 1,
    "prescriptionDate": "2026-08-31",
    "items": [
      {
        "medicineId": 1,
        "medicineName": "Ibuprofen",
        "dosage": "500mg",
        "frequency": "Twice daily"
      },
      ...
    ]
  }
}
```

---

### Phase G: Invoice & Payment Tests

#### Test 17: Create Invoice
```bash
curl -X POST https://localhost:7123/api/invoices \
  -H "Authorization: Bearer {RECEPTIONIST_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "patientId": 1,
    "appointmentId": 1,
    "items": [
      {
        "description": "Cardiology Consultation",
        "quantity": 1,
        "unitPrice": 250.00
      }
    ],
    "taxPercentage": 10,
    "discountAmount": 0
  }'
```

**Expected Response** (201 Created):
```json
{
  "success": true,
  "message": "Invoice created successfully.",
  "data": {
    "id": 5,
    "invoiceNumber": "INV-2026-00005",
    "patientId": 1,
    "subtotal": 250.00,
    "taxAmount": 25.00,
    "totalAmount": 275.00,
    "paymentStatus": "Pending"
  }
}
```

#### Test 18: Record Payment
```bash
curl -X POST https://localhost:7123/api/invoices/payments \
  -H "Authorization: Bearer {RECEPTIONIST_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "invoiceId": 5,
    "amount": 275.00,
    "paymentMethod": "Credit Card",
    "transactionReference": "TXN-2026-00001"
  }'
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "message": "Payment recorded successfully.",
  "data": {
    "id": 1,
    "invoiceId": 5,
    "amount": 275.00,
    "paymentMethod": "Credit Card",
    "paymentDate": "2026-08-31"
  }
}
```

---

### Phase H: Dashboard Tests

#### Test 19: Get Admin Dashboard
```bash
curl -X GET https://localhost:7123/api/dashboard/admin \
  -H "Authorization: Bearer {ADMIN_TOKEN}"
```

**Expected Response** (200 OK):
```json
{
  "success": true,
  "data": {
    "totalPatients": 5,
    "totalDoctors": 3,
    "todaysAppointments": 3,
    "upcomingAppointments": 4,
    "totalRevenue": 1050.00,
    "pendingPayments": 275.00,
    "appointmentStats": {
      "completed": 2,
      "cancelled": 1,
      "noshow": 0
    }
  }
}
```

#### Test 20: Get Patient Dashboard
```bash
curl -X GET https://localhost:7123/api/dashboard/patient/1 \
  -H "Authorization: Bearer {PATIENT_TOKEN}"
```

---

## Authorization Tests

### Test 21: Unauthorized Access (No Token)
```bash
curl -X GET https://localhost:7123/api/patients
```

**Expected Response** (401 Unauthorized):
```json
{
  "success": false,
  "message": "Authorization token missing or invalid."
}
```

### Test 22: Forbidden Access (Wrong Role)
```bash
# Patient trying to delete another patient
curl -X DELETE https://localhost:7123/api/patients/1 \
  -H "Authorization: Bearer {PATIENT_TOKEN}"
```

**Expected Response** (403 Forbidden):
```json
{
  "success": false,
  "message": "You do not have permission to perform this action."
}
```

---

## Validation Error Tests

### Test 23: Invalid Email
```bash
curl -X POST https://localhost:7123/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "invalid-email",
    "password": "Admin@123"
  }'
```

**Expected Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": ["Email must be a valid email address"]
}
```

### Test 24: Duplicate Email
```bash
curl -X POST https://localhost:7123/api/patients \
  -H "Authorization: Bearer {ADMIN_TOKEN}" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@email.com",  /* Already exists */
    "password": "NewPassword@123",
    "firstName": "Test",
    "lastName": "User",
    ...
  }'
```

**Expected Response** (409 Conflict):
```json
{
  "success": false,
  "message": "A user with this email already exists."
}
```

---

## Postman Collection

### Import into Postman:

**Environment Variables** (Set these):
```
{{base_url}} = https://localhost:7123
{{token}} = [JWT token from login]
{{admin_token}} = [Admin user's JWT token]
{{patient_id}} = 1
{{doctor_id}} = 1
```

### Example Requests:

**1. Login (Get Token)**
```
POST {{base_url}}/api/auth/login
Body:
{
  "email": "admin@careflow.com",
  "password": "Admin@123"
}

Pre-request Script:
// Set token from response
pm.environment.set("token", pm.response.json().data.token);
```

**2. Get Patients**
```
GET {{base_url}}/api/patients?pageNumber=1&pageSize=10
Headers:
Authorization: Bearer {{token}}
```

**3. Create Patient**
```
POST {{base_url}}/api/patients
Headers:
Authorization: Bearer {{admin_token}}
Body:
{...patient data...}
```

---

## Swagger UI Testing

### Using Swagger UI (Recommended for Quick Testing)

1. Navigate to: `https://localhost:7123/swagger`
2. Click "Authorize" button (lock icon)
3. Paste JWT token from login response
4. Click "Authorize"
5. Try out endpoints directly in UI

### Swagger Features:
- Request/response examples
- Schema definitions
- Authorization testing
- Response code details

---

## Performance Testing

### Test 25: Large Pagination
```bash
curl -X GET "https://localhost:7123/api/patients?pageNumber=1&pageSize=1000" \
  -H "Authorization: Bearer {TOKEN}"
```

**Verify**: Response time < 2 seconds

### Test 26: Concurrent Requests
```bash
# Run 10 concurrent requests
for i in {1..10}; do
  curl -X GET "https://localhost:7123/api/patients?pageNumber=1&pageSize=10" \
    -H "Authorization: Bearer {TOKEN}" &
done
```

---

## Testing Checklist

- [ ] Authentication tests pass (Login, Register, Get Current User)
- [ ] Patient CRUD working
- [ ] Doctor endpoints accessible
- [ ] Appointments can be booked, confirmed, cancelled
- [ ] Medical records creation working
- [ ] Prescriptions with items saving
- [ ] Invoices and payments recording
- [ ] Dashboard endpoints returning data
- [ ] Role-based authorization enforced
- [ ] Validation errors returning 400
- [ ] Unauthorized requests returning 401
- [ ] Forbidden requests returning 403
- [ ] Not found returning 404
- [ ] Swagger UI accessible
- [ ] All endpoints documented in Swagger

---

## Troubleshooting

### Issue: SSL Certificate Error
**Solution**:
```bash
# For development, skip certificate validation
# In curl:
curl -k https://localhost:7123/api/auth/login ...

# In Postman:
Settings → General → Turn off "SSL certificate verification"
```

### Issue: CORS Error
**Solution**: Ensure CORS is configured in Program.cs (already done)

### Issue: Token Expired
**Solution**: Login again to get new token

### Issue: 500 Internal Server Error
**Solution**: Check API logs for detailed error message

---

**All Tests Ready! Start with Test 1 and proceed sequentially.** ✓
