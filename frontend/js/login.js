document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector("form");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const username = document.getElementById("email").value.trim();
        const password = document.getElementById("password").value;

        try {
            const respuesta = await fetch(`${API_URL}/Auth/login`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password })
            });

            if (!respuesta.ok) {
                alert("Credenciales inválidas. Verifica tu usuario y contraseña.");
                return;
            }

            const datos = await respuesta.json();
            guardarSesion(datos.token, datos.usuario);

            if (datos.usuario.role === "Admin") {
                window.location.href = "admin.html";
            } else {
                window.location.href = "dashboard.html";
            }

        } catch (error) {
            console.error("Error al iniciar sesión:", error);
            alert("No se pudo conectar con el servidor. Verifica que el API esté corriendo.");
        }
    });
});