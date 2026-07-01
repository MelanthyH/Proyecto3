document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector("form");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const username = document.getElementById("username").value.trim();
        const email = document.getElementById("email").value.trim();
        const password = document.getElementById("password").value;

        try {
            const respuesta = await fetch(`${API_URL}/Auth/register`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, email, password })
            });

            if (!respuesta.ok) {
                const errorTexto = await respuesta.text();
                alert("No se pudo registrar: " + errorTexto);
                return;
            }

            alert("¡Cuenta creada correctamente! Ahora puedes iniciar sesión.");
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            localStorage.removeItem('usuario');
            window.location.href = "login.html";

        } catch (error) {
            console.error("Error al registrar:", error);
            alert("No se pudo conectar con el servidor. Verifica que el API esté corriendo.");
        }
    });
});