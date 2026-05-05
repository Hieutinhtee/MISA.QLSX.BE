# UML Sequence Diagrams — Dự án MISA.QLSX

> **Quy ước cột:**
> - Cột 1: `Client` — trình duyệt / người dùng
> - Cột 2: `[Tên]UI` — component / trang giao diện
> - Cột 3: `[Tên]Controller` — API Controller (.NET)
> - Cột 4: `[Tên]Service` — Business Service
> - Cột 5: `[Tên]Repository` — Data Access / Entity
>
> **Ký hiệu:**
> - `n-m: method()` — gọi đồng bộ từ cột n sang cột m
> - `n--m: method()` — gọi bất đồng bộ (async) từ cột n sang cột m
> - `n-n: method()` — tự gọi (self-call), thường là UI render sau khi controller trả về

---

## UC-01: Xác thực người dùng (Đăng nhập / Đăng xuất)

> **Cột:** 1-Client | 2-LoginUI | 3-AuthController | 4-AccountService | 5-AccountRepository

### Đăng nhập

```
title: UC-01 - Đăng nhập

1-2: navigate("/login")
2-2: renderLoginForm()
1-2: submitLoginForm(username, password)
2-3: POST /api/auth/login { username, password }
3--4: LoginAsync(LoginRequest)
4--5: FindByUsernameAsync(username)
5--4: Account | null
4-4: verifyPassword(password, passwordHash)
4--5: UpdateLastLoginAt(accountId)
5--4: ok
4--3: LoginResponse { token, accountInfo }
3-2: 200 OK { token, accountInfo }
2-2: saveToken(), redirectToDashboard()
```

### Đăng xuất

```
title: UC-01 - Đăng xuất

1-2: clickLogout()
2-3: POST /api/auth/logout
3-2: 200 OK
2-2: clearToken(), navigate("/login")
```

---

## UC-02: Quản lý nhân viên

> **Cột:** 1-Client | 2-EmployeeUI | 3-EmployeesController | 4-EmployeeService | 5-EmployeeRepository

### Xem danh sách (có phân trang)

```
title: UC-02 - Xem danh sách nhân viên

1-2: navigate("/employees")
2-2: renderPage()
2-3: POST /api/v1/employees/paging { page, pageSize, search, filters }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Employee>
4--3: PagingResponse<Employee>
3-2: 200 OK { data: PagingResponse }
2-2: renderTable(employees)
```

### Xem chi tiết nhân viên

```
title: UC-02 - Xem chi tiết nhân viên

1-2: clickRow(employeeId)
2-3: GET /api/v1/employees/{id}
3--4: GetByIdAsync(id)
4--5: GetById(id)
5--4: Employee
4--3: Employee
3-2: 200 OK { data: Employee }
2-2: renderDetailForm(employee)
```

### Thêm nhân viên

```
title: UC-02 - Thêm nhân viên mới

1-2: clickAddNew()
2-2: renderCreateForm()
1-2: submitForm(employeeData)
2-3: POST /api/v1/employees { employeeData }
3--4: CreateAsync(Employee)
4-4: ValidateAsync(employee)
4--5: IsValueExistAsync("EmployeeCode", code)
5--4: false
4--5: IsValueExistAsync("account_code", code) [AccountRepository]
5--4: false
4--5: InsertAsync(Account) [AccountRepository]
5--4: accountId
4--5: InsertAsync(Employee)
5--4: employeeId
4--3: employeeId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Cập nhật nhân viên

```
title: UC-02 - Cập nhật nhân viên

1-2: submitEditForm(id, employeeData)
2-3: PUT /api/v1/employees/{id} { employeeData }
3--4: UpdateAsync(id, Employee)
4--5: GetById(id)
5--4: Employee
4-4: ValidateAsync(employee, id)
4--5: IsValueExistAsync("EmployeeCode", code, ignoreId)
5--4: false
4--5: UpdateAsync(id, Employee)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa nhân viên (đơn / hàng loạt)

```
title: UC-02 - Xóa nhân viên

1-2: clickDelete(id) | selectMany + clickBatchDelete([ids])
2-2: showConfirmDialog()
1-2: confirmDelete()
2-3: DELETE /api/v1/employees/{id}  |  POST /api/v1/employees/batch-delete [ids]
3--4: DeleteAsync(id)  |  DeleteManyAsync([ids])
4--5: DeleteAsync(id)  |  DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { deleted }
2-2: showSuccessNotification(), reloadTable()
```

### Xuất Excel

```
title: UC-02 - Xuất Excel danh sách nhân viên

1-2: selectRows([ids]), clickExport()
2-3: POST /api/v1/employees/export-selected [ids]
3--4: ExportSelectedExcelAsync([ids])
4--5: GetAllAsync()
5--4: List<Employee>
4-4: filterByIds(), buildExcel()
4--3: byte[] fileContent
3-2: 200 OK (file .xlsx)
2-2: downloadFile()
```

---

## UC-03: Quản lý ca làm việc

> **Cột:** 1-Client | 2-ShiftUI | 3-ShiftsController | 4-ShiftService | 5-ShiftRepository

### Xem danh sách

```
title: UC-03 - Xem danh sách ca làm việc

1-2: navigate("/shifts")
2-2: renderPage()
2-3: POST /api/v1/shifts/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Shift>
4--3: PagingResponse<Shift>
3-2: 200 OK { data }
2-2: renderTable(shifts)
```

### Thêm / Sửa / Xóa ca làm việc

```
title: UC-03 - Thêm ca làm việc

1-2: submitCreateForm(shiftData)
2-3: POST /api/v1/shifts { shiftData }
3--4: CreateAsync(Shift)
4-4: ValidateAsync(shift)
4--5: InsertAsync(Shift)
5--4: shiftId
4--3: shiftId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-03 - Cập nhật ca làm việc

1-2: submitEditForm(id, shiftData)
2-3: PUT /api/v1/shifts/{id} { shiftData }
3--4: UpdateAsync(id, Shift)
4--5: GetById(id)
5--4: Shift
4-4: ValidateAsync(shift, id)
4--5: UpdateAsync(id, Shift)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-03 - Xóa ca làm việc

1-2: confirmDelete([ids])
2-3: POST /api/v1/shifts/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

### Kích hoạt / Vô hiệu hóa hàng loạt

```
title: UC-03 - Kích hoạt / Vô hiệu hóa ca làm việc

1-2: selectMany([ids]), clickToggleActive(isActive)
2-3: PUT /api/v1/shifts/batch-active { ids, isActive }
3--4: BulkUpdateSameValueAsync([ids], "is_active", isActive)
4--5: BulkUpdateSameValueAsync([ids], "is_active", isActive)
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xuất Excel

```
title: UC-03 - Xuất Excel ca làm việc

1-2: clickExportAll()
2-3: GET /api/v1/shifts/export
3--4: ExportShiftAsync()
4--5: GetAllAsync()
5--4: List<Shift>
4-4: buildExcel()
4--3: byte[] fileContent
3-2: 200 OK (file .xlsx)
2-2: downloadFile()
```

---

## UC-04: Quản lý phòng ban

> **Cột:** 1-Client | 2-DepartmentUI | 3-DepartmentsController | 4-DepartmentService | 5-DepartmentRepository

### Xem danh sách

```
title: UC-04 - Xem danh sách phòng ban

1-2: navigate("/departments")
2-2: renderPage()
2-3: POST /api/v1/departments/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Department>
4--3: PagingResponse<Department>
3-2: 200 OK { data }
2-2: renderTable(departments)
```

### Thêm phòng ban

```
title: UC-04 - Thêm phòng ban

1-2: submitForm(departmentData)
2-3: POST /api/v1/departments { departmentData }
3--4: CreateAsync(Department)
4-4: ValidateAsync(department)
4--5: InsertAsync(Department)
5--4: departmentId
4--3: departmentId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Sửa phòng ban

```
title: UC-04 - Cập nhật phòng ban

1-2: submitEditForm(id, departmentData)
2-3: PUT /api/v1/departments/{id} { departmentData }
3--4: UpdateAsync(id, Department)
4--5: GetById(id)
5--4: Department
4-4: ValidateAsync(department, id)
4--5: UpdateAsync(id, Department)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa phòng ban

```
title: UC-04 - Xóa phòng ban

1-2: confirmDelete([ids])
2-3: POST /api/v1/departments/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-05: Quản lý chức vụ

> **Cột:** 1-Client | 2-PositionUI | 3-PositionsController | 4-PositionService | 5-PositionRepository

### Xem danh sách

```
title: UC-05 - Xem danh sách chức vụ

1-2: navigate("/positions")
2-2: renderPage()
2-3: POST /api/v1/positions/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Position>
4--3: PagingResponse<Position>
3-2: 200 OK { data }
2-2: renderTable(positions)
```

### Thêm / Sửa / Xóa chức vụ

```
title: UC-05 - Thêm chức vụ

1-2: submitForm(positionData)
2-3: POST /api/v1/positions { positionData }
3--4: CreateAsync(Position)
4-4: ValidateAsync(position)
4--5: InsertAsync(Position)
5--4: positionId
4--3: positionId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-05 - Cập nhật chức vụ

1-2: submitEditForm(id, positionData)
2-3: PUT /api/v1/positions/{id} { positionData }
3--4: UpdateAsync(id, Position)
4--5: GetById(id)
5--4: Position
4-4: ValidateAsync(position, id)
4--5: UpdateAsync(id, Position)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-05 - Xóa chức vụ

1-2: confirmDelete([ids])
2-3: POST /api/v1/positions/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-06: Quản lý bằng cấp

> **Cột:** 1-Client | 2-DegreeUI | 3-DegreesController | 4-DegreeService | 5-DegreeRepository

### Xem danh sách

```
title: UC-06 - Xem danh sách bằng cấp

1-2: navigate("/degrees")
2-2: renderPage()
2-3: POST /api/v1/degrees/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Degree>
4--3: PagingResponse<Degree>
3-2: 200 OK { data }
2-2: renderTable(degrees)
```

### Thêm / Sửa / Xóa bằng cấp

```
title: UC-06 - Thêm bằng cấp

1-2: submitForm(degreeData)
2-3: POST /api/v1/degrees { degreeData }
3--4: CreateAsync(Degree)
4-4: ValidateAsync(degree)
4--5: InsertAsync(Degree)
5--4: degreeId
4--3: degreeId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-06 - Cập nhật bằng cấp

1-2: submitEditForm(id, degreeData)
2-3: PUT /api/v1/degrees/{id} { degreeData }
3--4: UpdateAsync(id, Degree)
4--5: GetById(id)
5--4: Degree
4-4: ValidateAsync(degree, id)
4--5: UpdateAsync(id, Degree)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-06 - Xóa bằng cấp

1-2: confirmDelete([ids])
2-3: POST /api/v1/degrees/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-07: Quản lý phụ cấp

> **Cột:** 1-Client | 2-AllowanceUI | 3-AllowancesController | 4-AllowanceService | 5-AllowanceRepository

### Xem danh sách

```
title: UC-07 - Xem danh sách phụ cấp

1-2: navigate("/allowances")
2-2: renderPage()
2-3: POST /api/v1/allowances/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Allowance>
4--3: PagingResponse<Allowance>
3-2: 200 OK { data }
2-2: renderTable(allowances)
```

### Thêm phụ cấp

```
title: UC-07 - Thêm phụ cấp

1-2: submitForm(allowanceData)
2-3: POST /api/v1/allowances { allowanceData }
3--4: CreateAsync(Allowance)
4-4: ValidateAsync(allowance)
4--5: InsertAsync(Allowance)
5--4: allowanceId
4--3: allowanceId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Sửa phụ cấp

```
title: UC-07 - Cập nhật phụ cấp

1-2: submitEditForm(id, allowanceData)
2-3: PUT /api/v1/allowances/{id} { allowanceData }
3--4: UpdateAsync(id, Allowance)
4--5: GetById(id)
5--4: Allowance
4-4: ValidateAsync(allowance, id)
4--5: UpdateAsync(id, Allowance)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa phụ cấp

```
title: UC-07 - Xóa phụ cấp

1-2: confirmDelete([ids])
2-3: POST /api/v1/allowances/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-08: Quản lý mẫu hợp đồng

> **Cột:** 1-Client | 2-ContractTemplateUI | 3-ContractTemplatesController | 4-ContractTemplateService | 5-ContractTemplateRepository

### Xem danh sách

```
title: UC-08 - Xem danh sách mẫu hợp đồng

1-2: navigate("/contract-templates")
2-2: renderPage()
2-3: POST /api/v1/contracttemplates/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<ContractTemplate>
4--3: PagingResponse<ContractTemplate>
3-2: 200 OK { data }
2-2: renderTable(templates)
```

### Thêm mẫu hợp đồng

```
title: UC-08 - Thêm mẫu hợp đồng

1-2: submitForm(templateData)
2-3: POST /api/v1/contracttemplates { templateData }
3--4: CreateAsync(ContractTemplate)
4-4: ValidateAsync(template)
4--5: InsertAsync(ContractTemplate)
5--4: templateId
4--3: templateId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Sửa mẫu hợp đồng

```
title: UC-08 - Cập nhật mẫu hợp đồng

1-2: submitEditForm(id, templateData)
2-3: PUT /api/v1/contracttemplates/{id} { templateData }
3--4: UpdateAsync(id, ContractTemplate)
4--5: GetById(id)
5--4: ContractTemplate
4-4: ValidateAsync(template, id)
4--5: UpdateAsync(id, ContractTemplate)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa mẫu hợp đồng

```
title: UC-08 - Xóa mẫu hợp đồng

1-2: confirmDelete([ids])
2-3: POST /api/v1/contracttemplates/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-09: Quản lý hợp đồng

> **Cột:** 1-Client | 2-ContractUI | 3-ContractsController | 4-ContractService | 5-ContractRepository

### Xem danh sách

```
title: UC-09 - Xem danh sách hợp đồng

1-2: navigate("/contracts")
2-2: renderPage()
2-3: POST /api/v1/contracts/paging { page, pageSize, search, filters }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Contract>
4--3: PagingResponse<Contract>
3-2: 200 OK { data }
2-2: renderTable(contracts)
```

### Mở form tạo hợp đồng (nạp dữ liệu phụ)

```
title: UC-09 - Nạp dữ liệu form tạo hợp đồng

1-2: clickAddNew()
2-2: renderCreateForm()
2-3: GET /api/v1/employees/without-contract
3--4: GetEmployeesWithoutContractAsync()
4--5: GetAllAsync()
5--4: List<Employee>
4-4: filterContractId == null
4--3: List<Employee>
3-2: 200 OK { data: employees }
2-3: GET /api/v1/contracttemplates
3--4: GetAllAsync()
4--5: GetAllAsync()
5--4: List<ContractTemplate>
4--3: List<ContractTemplate>
3-2: 200 OK { data: templates }
2-3: GET /api/v1/employees/representatives
3--4: GetRepresentativesAsync()
4--5: GetEmployeesByRoleAsync("HR")
5--4: List<Employee>
4--3: List<Employee>
3-2: 200 OK { data: representatives }
2-2: renderFormWithData()
```

### Thêm hợp đồng

```
title: UC-09 - Thêm hợp đồng mới

1-2: submitForm(contractData)
2-3: POST /api/v1/contracts { contractData }
3--4: CreateAsync(Contract)
4-4: ValidateAsync(contract)
4-4: checkContractCode, checkEmployee, checkRepresentative, checkSalary...
4--5: IsValueExistAsync("ContractCode", code)
5--4: false
4-4: BeforeSaveAsync(contract)
4-4: setSignedAt() if isSigned
4-4: setStatus("active") if isSigned
4--5: InsertAsync(Contract)
5--4: contractId
4--3: contractId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Cập nhật hợp đồng

```
title: UC-09 - Cập nhật hợp đồng

1-2: submitEditForm(id, contractData)
2-3: PUT /api/v1/contracts/{id} { contractData }
3--4: UpdateAsync(id, Contract)
4--5: GetById(id)
5--4: Contract
4-4: ValidateAsync(contract, id)
4-4: BeforeSaveAsync(contract, isUpdate=true)
4--5: UpdateAsync(id, Contract)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa hợp đồng

```
title: UC-09 - Xóa hợp đồng

1-2: confirmDelete([ids])
2-3: POST /api/v1/contracts/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-10: Quản lý chấm công

> **Cột:** 1-Client | 2-AttendanceUI | 3-AttendancesController | 4-AttendanceService | 5-AttendanceRepository

### Xem danh sách

```
title: UC-10 - Xem danh sách chấm công

1-2: navigate("/attendances")
2-2: renderPage()
2-3: POST /api/v1/attendances/paging { page, pageSize, search, filters }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Attendance>
4--3: PagingResponse<Attendance>
3-2: 200 OK { data }
2-2: renderTable(attendances)
```

### Thêm chấm công

```
title: UC-10 - Thêm bản ghi chấm công

1-2: submitForm(attendanceData)
2-3: POST /api/v1/attendances { attendanceData }
3--4: CreateAsync(Attendance)
4-4: ValidateAsync(attendance)
4-4: checkCode, checkEmployee, checkDate, checkHours, checkAmount...
4--5: IsValueExistAsync("AttendanceCode", code)
5--4: false
4-4: BeforeSaveAsync(attendance)
4--5: InsertAsync(Attendance)
5--4: attendanceId
4--3: attendanceId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Cập nhật chấm công

```
title: UC-10 - Cập nhật chấm công

1-2: submitEditForm(id, attendanceData)
2-3: PUT /api/v1/attendances/{id} { attendanceData }
3--4: UpdateAsync(id, Attendance)
4--5: GetById(id)
5--4: Attendance
4-4: ValidateAsync(attendance, id)
4--5: UpdateAsync(id, Attendance)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa chấm công

```
title: UC-10 - Xóa chấm công

1-2: confirmDelete([ids])
2-3: POST /api/v1/attendances/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

### Xem Dashboard chấm công

```
title: UC-10 - Xem Dashboard chấm công

1-2: navigate("/attendances/dashboard")
2-2: renderDashboardPage()
2-3: GET /api/v1/attendances/dashboard?date=2025-01-15
3--4: GetAttendanceDashboard(date)
4--5: GetAbsentEmployeesTodayAsync(date)
5--4: List<AbsentItem>
4--5: GetLateRankingsAsync(month, year)
5--4: List<LateRankingItem>
4-4: buildDashboardDto()
4--3: AttendanceDashboardDto
3-2: 200 OK { data: AttendanceDashboardDto }
2-2: renderDashboard(absents, lateRankings)
```

### Xem lịch chấm công nhân viên

```
title: UC-10 - Xem lịch chấm công theo tháng

1-2: selectEmployee(employeeId), selectMonth(month, year)
2-3: GET /api/v1/attendances/employee/{employeeId}/calendar?month=1&year=2025
3--4: GetEmployeeCalendar(employeeId, month, year)
4--5: GetAttendancesByEmployeeInMonthAsync(employeeId, month, year)
5--4: List<Attendance>
4-4: buildCalendarDto(), calcSummary()
4--3: AttendanceCalendarDto
3-2: 200 OK { data: AttendanceCalendarDto }
2-2: renderCalendar(records, summary)
```

---

## UC-11: Quản lý công tác

> **Cột:** 1-Client | 2-BusinessTripUI | 3-BusinessTripsController | 4-BusinessTripService | 5-BusinessTripRepository

### Xem danh sách

```
title: UC-11 - Xem danh sách công tác

1-2: navigate("/business-trips")
2-2: renderPage()
2-3: POST /api/v1/businesstrips/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<BusinessTrip>
4--3: PagingResponse<BusinessTrip>
3-2: 200 OK { data }
2-2: renderTable(trips)
```

### Thêm công tác

```
title: UC-11 - Thêm chuyến công tác

1-2: submitForm(tripData)
2-3: POST /api/v1/businesstrips { tripData }
3--4: CreateAsync(BusinessTrip)
4-4: ValidateAsync(trip)
4--5: InsertAsync(BusinessTrip)
5--4: tripId
4--3: tripId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Cập nhật công tác

```
title: UC-11 - Cập nhật chuyến công tác

1-2: submitEditForm(id, tripData)
2-3: PUT /api/v1/businesstrips/{id} { tripData }
3--4: UpdateAsync(id, BusinessTrip)
4--5: GetById(id)
5--4: BusinessTrip
4-4: ValidateAsync(trip, id)
4--5: UpdateAsync(id, BusinessTrip)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa công tác

```
title: UC-11 - Xóa chuyến công tác

1-2: confirmDelete([ids])
2-3: POST /api/v1/businesstrips/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-12: Quản lý đánh giá nhân viên

> **Cột:** 1-Client | 2-EvaluationUI | 3-EvaluationsController | 4-EvaluationService | 5-EvaluationRepository

### Xem danh sách

```
title: UC-12 - Xem danh sách đánh giá

1-2: navigate("/evaluations")
2-2: renderPage()
2-3: POST /api/v1/evaluations/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Evaluation>
4--3: PagingResponse<Evaluation>
3-2: 200 OK { data }
2-2: renderTable(evaluations)
```

### Thêm đánh giá

```
title: UC-12 - Thêm đánh giá nhân viên

1-2: submitForm(evaluationData)
2-3: POST /api/v1/evaluations { evaluationData }
3--4: CreateAsync(Evaluation)
4-4: ValidateAsync(evaluation)
4--5: InsertAsync(Evaluation)
5--4: evaluationId
4--3: evaluationId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Cập nhật đánh giá

```
title: UC-12 - Cập nhật đánh giá

1-2: submitEditForm(id, evaluationData)
2-3: PUT /api/v1/evaluations/{id} { evaluationData }
3--4: UpdateAsync(id, Evaluation)
4--5: GetById(id)
5--4: Evaluation
4-4: ValidateAsync(evaluation, id)
4--5: UpdateAsync(id, Evaluation)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

### Xóa đánh giá

```
title: UC-12 - Xóa đánh giá

1-2: confirmDelete([ids])
2-3: POST /api/v1/evaluations/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-13: Quản lý kỳ lương

> **Cột:** 1-Client | 2-SalaryPeriodUI | 3-SalaryPeriodsController | 4-SalaryPeriodService | 5-SalaryPeriodRepository

### Xem danh sách

```
title: UC-13 - Xem danh sách kỳ lương

1-2: navigate("/salary-periods")
2-2: renderPage()
2-3: POST /api/v1/salaryperiods/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<SalaryPeriod>
4--3: PagingResponse<SalaryPeriod>
3-2: 200 OK { data }
2-2: renderTable(periods)
```

### Thêm / Sửa / Xóa kỳ lương

```
title: UC-13 - Thêm kỳ lương

1-2: submitForm(periodData)
2-3: POST /api/v1/salaryperiods { periodData }
3--4: CreateAsync(SalaryPeriod)
4-4: ValidateAsync(period)
4--5: InsertAsync(SalaryPeriod)
5--4: periodId
4--3: periodId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-13 - Cập nhật kỳ lương

1-2: submitEditForm(id, periodData)
2-3: PUT /api/v1/salaryperiods/{id} { periodData }
3--4: UpdateAsync(id, SalaryPeriod)
4--5: GetById(id)
5--4: SalaryPeriod
4-4: ValidateAsync(period, id)
4--5: UpdateAsync(id, SalaryPeriod)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-13 - Xóa kỳ lương

1-2: confirmDelete([ids])
2-3: POST /api/v1/salaryperiods/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-14: Quản lý chính sách lương

> **Cột:** 1-Client | 2-SalaryPolicyUI | 3-SalaryPoliciesController | 4-SalaryPolicyService | 5-SalaryPolicyRepository

### Xem danh sách

```
title: UC-14 - Xem danh sách chính sách lương

1-2: navigate("/salary-policies")
2-2: renderPage()
2-3: POST /api/v1/salarypolicies/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<SalaryPolicy>
4--3: PagingResponse<SalaryPolicy>
3-2: 200 OK { data }
2-2: renderTable(policies)
```

### Thêm / Sửa / Xóa chính sách lương

```
title: UC-14 - Thêm chính sách lương

1-2: submitForm(policyData)
2-3: POST /api/v1/salarypolicies { policyData }
3--4: CreateAsync(SalaryPolicy)
4-4: ValidateAsync(policy)
4--5: InsertAsync(SalaryPolicy)
5--4: policyId
4--3: policyId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-14 - Cập nhật chính sách lương

1-2: submitEditForm(id, policyData)
2-3: PUT /api/v1/salarypolicies/{id} { policyData }
3--4: UpdateAsync(id, SalaryPolicy)
4--5: GetById(id)
5--4: SalaryPolicy
4-4: ValidateAsync(policy, id)
4--5: UpdateAsync(id, SalaryPolicy)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-14 - Xóa chính sách lương

1-2: confirmDelete([ids])
2-3: POST /api/v1/salarypolicies/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-15: Quản lý chính sách khấu trừ

> **Cột:** 1-Client | 2-DeductionPolicyUI | 3-DeductionPoliciesController | 4-DeductionPolicyService | 5-DeductionPolicyRepository

### Xem danh sách

```
title: UC-15 - Xem danh sách chính sách khấu trừ

1-2: navigate("/deduction-policies")
2-2: renderPage()
2-3: POST /api/v1/deductionpolicies/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<DeductionPolicy>
4--3: PagingResponse<DeductionPolicy>
3-2: 200 OK { data }
2-2: renderTable(policies)
```

### Thêm / Sửa / Xóa chính sách khấu trừ

```
title: UC-15 - Thêm chính sách khấu trừ

1-2: submitForm(deductionData)
2-3: POST /api/v1/deductionpolicies { deductionData }
3--4: CreateAsync(DeductionPolicy)
4-4: ValidateAsync(deduction)
4--5: InsertAsync(DeductionPolicy)
5--4: policyId
4--3: policyId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-15 - Cập nhật chính sách khấu trừ

1-2: submitEditForm(id, deductionData)
2-3: PUT /api/v1/deductionpolicies/{id} { deductionData }
3--4: UpdateAsync(id, DeductionPolicy)
4--5: GetById(id)
5--4: DeductionPolicy
4-4: ValidateAsync(deduction, id)
4--5: UpdateAsync(id, DeductionPolicy)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-15 - Xóa chính sách khấu trừ

1-2: confirmDelete([ids])
2-3: POST /api/v1/deductionpolicies/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-16: Quản lý khung thuế thu nhập cá nhân

> **Cột:** 1-Client | 2-TaxBracketUI | 3-TaxBracketsController | 4-TaxBracketService | 5-TaxBracketRepository

### Xem danh sách

```
title: UC-16 - Xem danh sách khung thuế TNCN

1-2: navigate("/tax-brackets")
2-2: renderPage()
2-3: POST /api/v1/taxbrackets/paging { page, pageSize }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<TaxBracket>
4--3: PagingResponse<TaxBracket>
3-2: 200 OK { data }
2-2: renderTable(brackets)
```

### Thêm / Sửa / Xóa khung thuế

```
title: UC-16 - Thêm khung thuế TNCN

1-2: submitForm(bracketData)
2-3: POST /api/v1/taxbrackets { bracketData }
3--4: CreateAsync(TaxBracket)
4-4: ValidateAsync(bracket)
4--5: InsertAsync(TaxBracket)
5--4: bracketId
4--3: bracketId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-16 - Cập nhật khung thuế TNCN

1-2: submitEditForm(id, bracketData)
2-3: PUT /api/v1/taxbrackets/{id} { bracketData }
3--4: UpdateAsync(id, TaxBracket)
4--5: GetById(id)
5--4: TaxBracket
4-4: ValidateAsync(bracket, id)
4--5: UpdateAsync(id, TaxBracket)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-16 - Xóa khung thuế TNCN

1-2: confirmDelete([ids])
2-3: POST /api/v1/taxbrackets/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-17: Quản lý hồ sơ thuế nhân viên

> **Cột:** 1-Client | 2-EmployeeTaxProfileUI | 3-EmployeeTaxProfilesController | 4-EmployeeTaxProfileService | 5-EmployeeTaxProfileRepository

### Xem danh sách

```
title: UC-17 - Xem danh sách hồ sơ thuế nhân viên

1-2: navigate("/employee-tax-profiles")
2-2: renderPage()
2-3: POST /api/v1/employeetaxprofiles/paging { page, pageSize, search }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<EmployeeTaxProfile>
4--3: PagingResponse<EmployeeTaxProfile>
3-2: 200 OK { data }
2-2: renderTable(profiles)
```

### Thêm / Sửa / Xóa hồ sơ thuế

```
title: UC-17 - Thêm hồ sơ thuế nhân viên

1-2: submitForm(profileData)
2-3: POST /api/v1/employeetaxprofiles { profileData }
3--4: CreateAsync(EmployeeTaxProfile)
4-4: ValidateAsync(profile)
4--5: InsertAsync(EmployeeTaxProfile)
5--4: profileId
4--3: profileId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-17 - Cập nhật hồ sơ thuế nhân viên

1-2: submitEditForm(id, profileData)
2-3: PUT /api/v1/employeetaxprofiles/{id} { profileData }
3--4: UpdateAsync(id, EmployeeTaxProfile)
4--5: GetById(id)
5--4: EmployeeTaxProfile
4-4: ValidateAsync(profile, id)
4--5: UpdateAsync(id, EmployeeTaxProfile)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadTable()
```

```
title: UC-17 - Xóa hồ sơ thuế nhân viên

1-2: confirmDelete([ids])
2-3: POST /api/v1/employeetaxprofiles/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-18: Tính lương và quản lý bảng lương

> **Cột:** 1-Client | 2-PayrollUI | 3-PayrollsController | 4-PayrollService | 5-PayrollRepository

### Xem danh sách bảng lương

```
title: UC-18 - Xem danh sách bảng lương

1-2: navigate("/payrolls")
2-2: renderPage()
2-3: POST /api/v1/payrolls/paging { page, pageSize, salaryPeriodId }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<Payroll>
4--3: PagingResponse<Payroll>
3-2: 200 OK { data }
2-2: renderTable(payrolls)
```

### Tạo nháp bảng lương (Generate)

```
title: UC-18 - Tạo nháp bảng lương

1-2: selectPeriod(salaryPeriodId), clickGenerate()
2-2: showConfirmDialog()
1-2: confirmGenerate()
2-3: POST /api/v1/payrolls/periods/{salaryPeriodId}/generate
3--4: GenerateDraftPayrollsAsync(salaryPeriodId)
4--5: GetById(salaryPeriodId) [SalaryPeriodRepository]
5--4: SalaryPeriod
4--5: GetActiveContractsAsync() [ContractRepository]
5--4: List<Contract>
4-4: buildDraftPayrolls()
4--5: InsertManyAsync(List<Payroll>)
5--4: rowsInserted
4--3: rowsInserted
3-2: 200 OK { generated: n }
2-2: showSuccessNotification(), reloadTable()
```

### Tính lương (Calculate)

```
title: UC-18 - Tính lương cho kỳ

1-2: selectPeriod(salaryPeriodId), clickCalculate()
2-2: showConfirmDialog()
1-2: confirmCalculate()
2-3: POST /api/v1/payrolls/periods/{salaryPeriodId}/calculate
3--4: CalculatePayrollsAsync(salaryPeriodId, employeeId?)
4--5: GetDraftPayrollsByPeriodAsync(salaryPeriodId) [PayrollRepository]
5--4: List<Payroll>
4--5: GetById(contract.SalaryPolicyId) [SalaryPolicyRepository]
5--4: SalaryPolicy
4--5: GetById(contract.DeductionPolicyId) [DeductionPolicyRepository]
5--4: DeductionPolicy
4--5: GetByEmployeeIdAsync(employeeId) [EmployeeTaxProfileRepository]
5--4: EmployeeTaxProfile
4--5: GetAllAsync() [TaxBracketRepository]
5--4: List<TaxBracket>
4--5: GetAttendanceByPeriodAsync(employeeId, period) [AttendanceRepository]
5--4: List<Attendance>
4-4: calcWorkingDays()
4-4: calcGrossSalary()
4-4: calcInsuranceDeduction()
4-4: calcTaxableIncome()
4-4: calcPITTax(taxBrackets)
4-4: calcNetSalary()
4--5: UpdateAsync(payrollId, Payroll)
5--4: updatedId
4--5: InsertPayrollItemsAsync(List<PayrollItem>) [PayrollItemRepository]
5--4: ok
4--3: rowsCalculated
3-2: 200 OK { calculated: n }
2-2: showSuccessNotification(), reloadTable()
```

### Khóa bảng lương (Lock)

```
title: UC-18 - Khóa bảng lương

1-2: clickLock(salaryPeriodId)
2-2: showConfirmDialog()
1-2: confirmLock()
2-3: POST /api/v1/payrolls/periods/{salaryPeriodId}/lock
3--4: LockPayrollsAsync(salaryPeriodId)
4--5: GetPayrollsByPeriodAsync(salaryPeriodId)
5--4: List<Payroll>
4-4: setStatus("locked"), setLockedAt(now)
4--5: UpdateManyStatusAsync([ids], "locked", lockedAt)
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { locked: n }
2-2: showSuccessNotification(), reloadTable()
```

### Đánh dấu đã thanh toán (Pay)

```
title: UC-18 - Đánh dấu đã thanh toán

1-2: clickPay(salaryPeriodId)
2-2: showConfirmDialog()
1-2: confirmPay()
2-3: POST /api/v1/payrolls/periods/{salaryPeriodId}/pay
3--4: MarkPayrollsPaidAsync(salaryPeriodId)
4--5: GetLockedPayrollsByPeriodAsync(salaryPeriodId)
5--4: List<Payroll>
4-4: setStatus("paid"), setPaidAt(now)
4--5: UpdateManyStatusAsync([ids], "paid", paidAt)
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { paid: n }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-19: Quản lý khoản mục lương

> **Cột:** 1-Client | 2-PayrollItemUI | 3-PayrollItemsController | 4-PayrollItemService | 5-PayrollItemRepository

### Xem khoản mục theo phiếu lương

```
title: UC-19 - Xem khoản mục lương theo phiếu lương

1-2: clickViewDetail(payrollId)
2-2: renderPayrollItemPanel()
2-3: GET /api/v1/payrollitems/payroll/{payrollId}
3--4: GetByPayrollIdAsync(payrollId)
4--5: GetByPayrollIdAsync(payrollId)
5--4: List<PayrollItem>
4--3: List<PayrollItem>
3-2: 200 OK { data: payrollItems }
2-2: renderItemList(payrollItems)
```

### Thêm khoản mục lương

```
title: UC-19 - Thêm khoản mục lương

1-2: submitForm(itemData)
2-3: POST /api/v1/payrollitems { itemData }
3--4: CreateAsync(PayrollItem)
4-4: ValidateAsync(item)
4--5: InsertAsync(PayrollItem)
5--4: itemId
4--3: itemId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadItems()
```

### Cập nhật khoản mục lương

```
title: UC-19 - Cập nhật khoản mục lương

1-2: submitEditForm(id, itemData)
2-3: PUT /api/v1/payrollitems/{id} { itemData }
3--4: UpdateAsync(id, PayrollItem)
4--5: GetById(id)
5--4: PayrollItem
4-4: ValidateAsync(item, id)
4--5: UpdateAsync(id, PayrollItem)
5--4: updatedId
4--3: updatedId
3-2: 200 OK { updated }
2-2: showSuccessNotification(), reloadItems()
```

### Xóa khoản mục lương

```
title: UC-19 - Xóa khoản mục lương

1-2: confirmDelete([ids])
2-3: POST /api/v1/payrollitems/batch-delete [ids]
3--4: DeleteManyAsync([ids])
4--5: DeleteManyAsync([ids])
5--4: rowsAffected
4--3: rowsAffected
3-2: 200 OK { TotalAffected }
2-2: showSuccessNotification(), reloadItems()
```

---

## UC-20: Quản lý yêu cầu phê duyệt

> **Cột:** 1-Client | 2-ApprovalRequestUI | 3-ApprovalRequestsController | 4-ApprovalRequestService | 5-ApprovalRequestRepository

### Xem danh sách yêu cầu

```
title: UC-20 - Xem danh sách yêu cầu phê duyệt

1-2: navigate("/approval-requests")
2-2: renderPage()
2-3: POST /api/v1/approvalrequests/paging { page, pageSize, search, status }
3--4: QueryPagingAsync(QueryRequest)
4--5: QueryPagingAsync(QueryRequest)
5--4: PagingResponse<ApprovalRequest>
4--3: PagingResponse<ApprovalRequest>
3-2: 200 OK { data }
2-2: renderTable(requests)
```

### Tạo yêu cầu phê duyệt

```
title: UC-20 - Tạo yêu cầu phê duyệt

1-2: submitForm(requestData)
2-3: POST /api/v1/approvalrequests { requestData }
3--4: CreateRequestAsync(ApprovalRequest)
4-4: validateFields(requestType, title, payload)
4--5: GenerateRequestCodeAsync()
5--4: requestCode
4-4: buildSteps(requestType)
4-4: setStatus("pending"), setCurrentStep(1)
4--5: InsertAsync(ApprovalRequest)
5--4: requestId
4--5: InsertStepAsync(ApprovalStep) [x N steps]
5--4: ok
4--3: requestId
3-2: 201 Created { id }
2-2: showSuccessNotification(), reloadTable()
```

### Xem các bước phê duyệt của yêu cầu

```
title: UC-20 - Xem các bước phê duyệt

1-2: clickViewSteps(requestId)
2-3: GET /api/v1/approvalrequests/{id}/steps
3--4: GetStepsAsync(requestId)
4--5: GetStepsByRequestIdAsync(requestId)
5--4: List<ApprovalStep>
4--3: List<ApprovalStep>
3-2: 200 OK { data: steps }
2-2: renderStepList(steps)
```

### Phê duyệt một bước

```
title: UC-20 - Phê duyệt bước

1-2: clickApprove(requestId, stepId, comment)
2-2: showConfirmDialog()
1-2: confirmApprove()
2-3: POST /api/v1/approvalrequests/{id}/approve/{stepId} { comment, actedBy }
3--4: ApproveStepAsync(requestId, stepId, comment, actedBy)
4--5: GetById(requestId)
5--4: ApprovalRequest
4-4: checkStatus("pending")
4--5: GetStepByIdAsync(stepId)
5--4: ApprovalStep
4-4: checkStepOrder(), checkStepStatus()
4-4: setStep("approved"), setActedAt, setActedBy
4--5: UpdateStepAsync(step)
5--4: ok
4-4: isLastStep?
4--5: UpdateAsync(requestId, { status: "approved" })
5--4: ok
4--4: ExecuteApprovedChangeAsync(request)
4--5: UpdateEmployeeDepartmentAsync() | UpdateDepartmentManagerAsync()
5--4: ok
4--3: true
3-2: 200 OK { success: true }
2-2: showSuccessNotification(), reloadTable()
```

### Từ chối một bước

```
title: UC-20 - Từ chối bước phê duyệt

1-2: clickReject(requestId, stepId, comment)
2-2: showConfirmDialog()
1-2: confirmReject()
2-3: POST /api/v1/approvalrequests/{id}/reject/{stepId} { comment, actedBy }
3--4: RejectStepAsync(requestId, stepId, comment, actedBy)
4--5: GetById(requestId)
5--4: ApprovalRequest
4-4: checkStatus("pending")
4--5: GetStepByIdAsync(stepId)
5--4: ApprovalStep
4-4: checkStepStatus()
4-4: setStep("rejected"), setActedAt, setActedBy
4--5: UpdateStepAsync(step)
5--4: ok
4--5: UpdateAsync(requestId, { status: "rejected" })
5--4: ok
4--3: true
3-2: 200 OK { success: true }
2-2: showSuccessNotification(), reloadTable()
```

---

## UC-21: Xem tổng quan hệ thống (Dashboard)

> **Cột:** 1-Client | 2-DashboardUI | 3-DashboardController | 4-(DB trực tiếp) | 5-MySqlConnection

### Xem Dashboard theo kỳ

```
title: UC-21 - Xem Dashboard tổng quan

1-2: navigate("/dashboard")
2-2: renderPage()
1-2: selectPeriod("month" | "week" | "year")
2-3: GET /api/v1/dashboard?period=month
3--5: CreateConnection()
5--3: IDbConnection
3--5: COUNT employees
5--3: totalEmployees
3--5: SUM net_salary (JOIN salary_period)
5--3: totalSalary
3--5: COUNT DISTINCT employees on business_trip today
5--3: onBusinessTrip
3--5: COUNT contracts expiring in 30 days
5--3: contractsExpiring
3--5: COUNT employees without contract
5--3: withoutContract
3--5: COUNT unsigned contracts
5--3: unsignedContracts
3--5: COUNT employees at risk (KỶ LUẬT in period)
5--3: atRisk
3--5: COUNT new employees in period
5--3: newEmployees
3--5: SELECT expiring contracts list (LIMIT 10)
5--3: expiringList
3--5: SELECT no-contract employees list (LIMIT 10)
5--3: noContractList
3--5: SELECT unsigned contracts list (LIMIT 10)
5--3: unsignedList
3--5: SELECT at-risk employees list (LIMIT 10)
5--3: atRiskList
3--5: SELECT business trip employees (today, LIMIT 10)
5--3: tripList
3-2: 200 OK { data: DashboardResponse }
2-2: renderKPICards(), renderDetailLists()
```
