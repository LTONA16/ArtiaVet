// Navbar Scroll Effect y Mobile Menu
document.addEventListener('DOMContentLoaded', function() {
    const navbar = document.getElementById('navbar');
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const mobileMenu = document.getElementById('mobileMenu');
    const menuIcon = document.getElementById('menuIcon');
    const closeIcon = document.getElementById('closeIcon');
    const navLinks = document.querySelectorAll('.nav-link, .mobile-nav-link');

    // Scroll effect para el navbar
    let lastScroll = 0;
    
    window.addEventListener('scroll', () => {
        const currentScroll = window.pageYOffset;

        if (currentScroll > 50) {
            navbar.classList.add('bg-white', 'shadow-lg');
            navbar.classList.remove('bg-transparent');
            
            // Cambiar colores del texto cuando el navbar es blanco
            document.querySelectorAll('.navbar-text, .nav-link').forEach(el => {
                el.classList.remove('text-white');
                el.classList.add('text-gray-800');
            });
            
            // Cambiar color del botón de menú móvil
            if (mobileMenuBtn) {
                mobileMenuBtn.classList.remove('text-white');
                mobileMenuBtn.classList.add('text-gray-800');
            }
        } else {
            navbar.classList.remove('bg-white', 'shadow-lg');
            navbar.classList.add('bg-transparent');
            
            // Restaurar colores originales
            document.querySelectorAll('.navbar-text, .nav-link').forEach(el => {
                el.classList.remove('text-gray-800');
                el.classList.add('text-white');
            });
            
            // Restaurar color del botón de menú móvil
            if (mobileMenuBtn) {
                mobileMenuBtn.classList.remove('text-gray-800');
                mobileMenuBtn.classList.add('text-white');
            }
        }

        lastScroll = currentScroll;
    });

    // Toggle Mobile Menu
    if (mobileMenuBtn) {
        mobileMenuBtn.addEventListener('click', () => {
            const isOpen = mobileMenu.style.maxHeight && mobileMenu.style.maxHeight !== '0px';
            
            if (isOpen) {
                mobileMenu.style.maxHeight = '0px';
                menuIcon.classList.remove('hidden');
                closeIcon.classList.add('hidden');
            } else {
                mobileMenu.style.maxHeight = mobileMenu.scrollHeight + 'px';
                menuIcon.classList.add('hidden');
                closeIcon.classList.remove('hidden');
            }
        });
    }

    // Cerrar menú móvil al hacer click en un enlace
    navLinks.forEach(link => {
        link.addEventListener('click', () => {
            if (window.innerWidth < 768) {
                mobileMenu.style.maxHeight = '0px';
                menuIcon.classList.remove('hidden');
                closeIcon.classList.add('hidden');
            }
        });
    });

    // Smooth scroll para los enlaces
    navLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = link.getAttribute('href');
            const targetSection = document.querySelector(targetId);
            
            if (targetSection) {
                const navbarHeight = navbar.offsetHeight;
                const targetPosition = targetSection.offsetTop - navbarHeight;
                
                window.scrollTo({
                    top: targetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });

    // Resaltar enlace activo al hacer scroll
    window.addEventListener('scroll', () => {
        let current = '';
        const sections = document.querySelectorAll('section[id]');
        const navbarHeight = navbar.offsetHeight;
        
        sections.forEach(section => {
            const sectionTop = section.offsetTop - navbarHeight - 100;
            const sectionHeight = section.offsetHeight;
            
            if (window.pageYOffset >= sectionTop && 
                window.pageYOffset < sectionTop + sectionHeight) {
                current = section.getAttribute('id');
            }
        });

        navLinks.forEach(link => {
            link.classList.remove('text-[#1a7487]', 'font-bold');
            const href = link.getAttribute('href');
            if (href === `#${current}`) {
                link.classList.add('text-[#1a7487]', 'font-bold');
            }
        });
    });
});