// URL base de tu API .NET
const API_URL = "http://localhost:5217/api";

function guardarSesion(token, usuario) {
    localStorage.setItem("token", token);
    localStorage.setItem("usuario", JSON.stringify(usuario));

    const nombre = usuario.username || usuario.nombre || usuario.Username || usuario.name || '';
    let planRaw = usuario.plan || usuario.Plan || usuario.role || '';
    planRaw = (typeof planRaw === 'string') ? planRaw.trim().toLowerCase() : '';
    const plan = (planRaw === 'pro' || planRaw === 'p' || planRaw === 'premium') ? 'PRO' : 'FREE';

    localStorage.setItem("user", JSON.stringify({ nombre, plan }));
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