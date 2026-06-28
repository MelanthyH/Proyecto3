// URL base de tu API .NET
const API_URL = "http://localhost:5217/api";

function guardarSesion(token, usuario) {
    localStorage.setItem("token", token);
    localStorage.setItem("usuario", JSON.stringify(usuario));
}

function obtenerToken() {
    return localStorage.getItem("token");
}

function obtenerUsuario() {
    const data = localStorage.getItem("usuario");
    return data ? JSON.parse(data) : null;
}

function cerrarSesion() {
    localStorage.removeItem("token");
    localStorage.removeItem("usuario");
    window.location.href = "login.html";
}

function protegerPagina() {
    if (!obtenerToken()) {
        window.location.href = "login.html";
    }
}