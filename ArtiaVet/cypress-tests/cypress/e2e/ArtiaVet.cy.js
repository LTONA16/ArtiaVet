describe('Visitar el sitio principal', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.wait(1000)
  })
})

describe('Funcionamiento del boton inicio', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/nav/div/div[1]/div/a[1]').click()
    cy.wait(1000)
  })
})

describe('Funcionamiento del boton servicios', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/nav/div/div[1]/div/a[2]').click()
    cy.wait(1000)
  })
})

describe('Funcionamiento del boton equipo', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/nav/div/div[1]/div/a[3]').click()
    cy.wait(1000)
  })
})

describe('Funcionamiento del boton testimonios', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/nav/div/div[1]/div/a[4]').click()
    cy.wait(1000)
  })
})

describe('Funcionamiento del boton FAQ', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/nav/div/div[1]/div/a[5]').click()
    cy.wait(1000)
  })
})

describe('Funcionamiento del slider testimonios', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/nav/div/div[1]/div/a[4]').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[4]/div/div[2]/div/div[1]/div/div[1]/div/p').contains('"Excelente atención y trato hacia mi perrita. El doctor fue muy profesional y se nota que realmente le importa el bienestar de los animales. Las instalaciones están muy limpias y modernas. Definitivamente regresaré."')
    cy.xpath('//*[@id="nextBtn"]').click()
    cy.wait(500)
    cy.xpath('//*[@id="nextBtn"]').click()
    cy.wait(500)
    cy.xpath('//*[@id="nextBtn"]').click()
    cy.wait(500)
    cy.xpath('//*[@id="nextBtn"]').click()
    cy.wait(500)
    cy.xpath('/html/body/main/section[4]/div/div[2]/div/div[1]/div/div[1]/div/p').contains('"Excelente atención y trato hacia mi perrita. El doctor fue muy profesional y se nota que realmente le importa el bienestar de los animales. Las instalaciones están muy limpias y modernas. Definitivamente regresaré."')
    cy.wait(2000)
  })
})

describe('Funcionamiento del acordeon FAQ', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/nav/div/div[1]/div/a[5]').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[1]/button').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[1]/div/div').contains('Recomendamos agendar cita previa para garantizar una mejor atención y reducir tiempos de espera. Sin embargo, también atendemos urgencias sin cita. Puedes agendar fácilmente por WhatsApp o llamando a nuestro consultorio.')
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[2]/button').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[2]/div/div').contains('Para la primera consulta te recomendamos traer cualquier historial médico previo de tu mascota, cartilla de vacunación si la tienes, y una muestra de heces si es posible. También es útil anotar cualquier comportamiento inusual o síntoma que hayas observado.')
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[3]/button').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[3]/div/div').contains('Nuestro horario regular es de lunes a viernes de 9:00 a 19:00 y sábados de 9:00 a 14:00. Para emergencias fuera de horario, contamos con un servicio de atención telefónica que te orientará sobre los siguientes pasos a seguir.')
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[4]/button').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[4]/div/div').contains('Aceptamos efectivo, tarjetas de débito y crédito (Visa, Mastercard, American Express), y transferencias bancarias. Para cirugías y tratamientos extensos, también ofrecemos planes de pago que puedes consultar directamente con nosotros.')
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[5]/button').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[5]/div/div').contains('Sí, ofrecemos servicio de consultas a domicilio en casos especiales, como mascotas con movilidad limitada o de edad avanzada. Este servicio tiene un costo adicional y debe agendarse con anticipación. Contáctanos para más detalles.')
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[6]/button').click()
    cy.wait(1000)
    cy.xpath('/html/body/main/section[5]/div/div[2]/div[6]/div/div').contains('Una consulta general típicamente dura entre 20 y 30 minutos. Este tiempo nos permite realizar un examen completo, responder todas tus preguntas y explicar detalladamente cualquier diagnóstico o tratamiento necesario.')
    cy.wait(1000)
  })
})

describe('Funcionamiento del boton agendar cita por whatsapp', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/section[1]/div[2]/a').click()
    cy.wait(2000)
  })
})

describe('Funcionamiento del boton contacto por whatsapp', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')
    cy.xpath('/html/body/main/section[5]/div/div[3]/a')
      .scrollIntoView()
      .should('be.visible')
      .click()
    cy.wait(2000)
  })
})

describe('Funcionamiento del boton iniciar sesion', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/')

  })
})

describe('Funcionamiento del sitio de recepcionista', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/Recepcionista')
  })
})

describe('Funcionamiento del modal crear cita', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/Recepcionista')
  })
})

describe('Validacion de campos de modal crear cita', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/Recepcionista')
  })
})

describe('Validacion de salida del modal', () => {
  it('passes', () => {
    cy.visit('http://localhost:5272/Recepcionista')
  })
})