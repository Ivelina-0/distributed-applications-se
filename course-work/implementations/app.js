const api = '/api';
let token = localStorage.getItem('token') || '';

const tomorrow = new Date(Date.now() + 86400000).toISOString().slice(0, 10);
aDate.value = tomorrow;
sDate.value = tomorrow;
updateUser();

function headers() {
    return {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
    };
}

async function request(url, opt = {}) {
    const r = await fetch(api + url, {
        ...opt,
        headers: { ...headers(), ...(opt.headers || {}) }
    });

    if (!r.ok) {
        let e = await r.json().catch(() => ({ message: r.statusText }));
        throw new Error(e.message || e.title || 'Грешка');
    }

    return r.status === 204 ? null : r.json();
}

async function login() {
    try {
        const data = await request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({
                email: email.value,
                password: password.value
            })
        });

        token = data.token;
        localStorage.setItem('token', token);
        localStorage.setItem('user', `${data.fullName} (${data.role})`);

        authMsg.innerHTML = '<span class="text-success">Успешен вход</span>';
        updateUser();

        loadPatients();
        loadDentists();
        loadSchedules();
        loadAppointments();
    }
    catch (e) {
        authMsg.innerHTML = '<span class="text-danger">' + e.message + '</span>';
    }
}

function logout() {
    localStorage.clear();
    token = '';
    updateUser();
}

function updateUser() {
    userInfo.textContent = localStorage.getItem('user') || 'Не сте влезли';
}

function fmt(v) {
    if (v === true) return 'Да';
    if (v === false) return 'Не';
    return v ?? '';
}

function table(rows, cols, type) {
    return `<div class="table-responsive">
        <table class="table table-hover align-middle">
            <thead>
                <tr>
                    ${cols.map(c => `<th>${c[0]}</th>`).join('')}
                    <th>Действие</th>
                </tr>
            </thead>
            <tbody>
                ${rows.map(x => `<tr>
                    ${cols.map(c => `<td>${fmt(x[c[1]])}</td>`).join('')}
                    <td>
                        <button class="btn btn-sm btn-outline-danger" onclick="removeItem('${type}',${x.id})">
                            Delete
                        </button>
                    </td>
                </tr>`).join('')}
            </tbody>
        </table>
    </div>`;
}

async function loadPatients() {
    try {
        let d = await request('/patients?search=' + encodeURIComponent(patientSearch.value) + '&pageSize=20&sortBy=fullName');

        patientsList.innerHTML = table(d.items, [
            ['ID', 'id'],
            ['Име', 'fullName'],
            ['Email', 'email'],
            ['Телефон', 'phoneNumber'],
            ['Адрес', 'address'],
            ['Активен', 'isActive']
        ], 'patients');
    }
    catch (e) {
        patientsList.innerHTML = '<div class="alert alert-danger">' + e.message + '</div>';
    }
}

async function createPatient() {
    try {
        await request('/patients', {
            method: 'POST',
            body: JSON.stringify({
                fullName: pName.value,
                email: pEmail.value,
                phoneNumber: pPhone.value,
                address: pAddress.value,
                birthDate: null
            })
        });

        pName.value = pEmail.value = pPhone.value = pAddress.value = '';
        loadPatients();
    }
    catch (e) {
        alert(e.message);
    }
}

async function loadDentists() {
    try {
        let d = await request('/dentists?name=' + encodeURIComponent(dentistNameSearch.value) + '&specialty=' + encodeURIComponent(dentistSpecialtySearch.value) + '&pageSize=20');

        dentistsList.innerHTML = table(d.items, [
            ['ID', 'id'],
            ['Име', 'fullName'],
            ['Специалност', 'specialty'],
            ['Телефон', 'phoneNumber'],
            ['Email', 'email'],
            ['Цена', 'consultationPrice']
        ], 'dentists');
    }
    catch (e) {
        dentistsList.innerHTML = '<div class="alert alert-danger">' + e.message + '</div>';
    }
}

async function createDentist() {
    try {
        await request('/dentists', {
            method: 'POST',
            body: JSON.stringify({
                fullName: dName.value,
                specialty: dSpecialty.value,
                phoneNumber: dPhone.value,
                email: dEmail.value,
                bio: '',
                consultationPrice: +dPrice.value
            })
        });

        dName.value = dSpecialty.value = dPhone.value = dEmail.value = '';
        loadDentists();
    }
    catch (e) {
        alert(e.message);
    }
}

async function loadSchedules() {
    try {
        let d = await request('/schedules');

        schedulesList.innerHTML = table(d, [
            ['ID', 'id'],
            ['Стоматолог', 'dentistName'],
            ['Дата', 'date'],
            ['От', 'startTime'],
            ['До', 'endTime'],
            ['Активен', 'isAvailable'],
            ['Бележки', 'notes']
        ], 'schedules');
    }
    catch (e) {
        schedulesList.innerHTML = '<div class="alert alert-danger">' + e.message + '</div>';
    }
}

async function createSchedule() {
    try {
        await request('/schedules', {
            method: 'POST',
            body: JSON.stringify({
                dentistId: +sDentist.value,
                date: sDate.value,
                startTime: sStart.value.length === 5 ? sStart.value + ':00' : sStart.value,
                endTime: sEnd.value.length === 5 ? sEnd.value + ':00' : sEnd.value,
                isAvailable: true,
                notes: sNotes.value
            })
        });

        sNotes.value = '';
        loadSchedules();
    }
    catch (e) {
        alert(e.message);
    }
}

async function loadAppointments() {
    try {
        let d = await request('/appointments?pageSize=20&sortBy=appointmentDate&sortOrder=desc');

        appointmentsList.innerHTML = table(d.items, [
            ['ID', 'id'],
            ['Пациент', 'patientName'],
            ['Телефон', 'patientPhone'],
            ['Стоматолог', 'dentistName'],
            ['Дата', 'appointmentDate'],
            ['Час', 'appointmentTime'],
            ['Причина', 'reason'],
            ['Статус', 'status']
        ], 'appointments');
    }
    catch (e) {
        appointmentsList.innerHTML = '<div class="alert alert-danger">' + e.message + '</div>';
    }
}

async function createAppointment() {
    try {
        await request('/appointments', {
            method: 'POST',
            body: JSON.stringify({
                patientId: +aPatient.value,
                dentistId: +aDentist.value,
                appointmentDate: aDate.value,
                appointmentTime: aTime.value.length === 5 ? aTime.value + ':00' : aTime.value,
                reason: aReason.value
            })
        });

        aReason.value = '';
        loadAppointments();
    }
    catch (e) {
        alert(e.message);
    }
}

async function removeItem(type, id) {
    if (!confirm('Изтриване/отмяна?')) return;

    try {
        await request('/' + type + '/' + id, { method: 'DELETE' });

        if (type === 'patients') loadPatients();
        if (type === 'dentists') loadDentists();
        if (type === 'schedules') loadSchedules();
        if (type === 'appointments') loadAppointments();
    }
    catch (e) {
        alert(e.message);
    }
}