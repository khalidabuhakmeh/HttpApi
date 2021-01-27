import http from 'k6/http';
export default function () {
    const url = 'https://localhost:5001/person';
   
    const payload = {
        name: 'Khalid',
        birthday: '1983-07-15',
        count: 1
    };
    const params = {
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
    };

    http.post(url, payload, params);
}