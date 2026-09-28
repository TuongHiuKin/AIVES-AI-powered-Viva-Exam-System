(() => {
    "use strict";

    const host = document.getElementById("modalContainer");
    if (!host) return;

    async function openModal(url) {
        const response = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        });

        if (!response.ok) {
            throw new Error(`Không thể tải hộp thoại (${response.status}).`);
        }

        host.innerHTML = await response.text();
        const element = host.querySelector(".modal");
        if (!element) {
            throw new Error("Nội dung trả về không chứa modal.");
        }

        bootstrap.Modal.getOrCreateInstance(element).show();
    }

    document.addEventListener("click", async event => {
        const trigger = event.target.closest("[data-modal-url]");
        if (!trigger) return;

        event.preventDefault();
        trigger.setAttribute("aria-busy", "true");
        try {
            await openModal(trigger.dataset.modalUrl);
        } catch (error) {
            console.error(error);
        } finally {
            trigger.removeAttribute("aria-busy");
        }
    });

    document.addEventListener("hidden.bs.modal", event => {
        if (host.contains(event.target)) {
            host.replaceChildren();
        }
    });

    window.AivesModal = { open: openModal };
})();
