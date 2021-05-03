using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CodeChallengeAPI.Data;
using CodeChallengeAPI.Models;
using RESTCountries.Services;
using ImageMagick;
using System.Net;
using System.Text;
using SharpVectors.Converters;
using Microsoft.Extensions.Logging;

namespace CodeChallengeAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class CountriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ApplicationDbContext context, ILogger<CountriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Por medio de este metodo se obtiene la lista entera de todos los paises
        /// </summary>
        [HttpGet("paises")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Obteniendo los paises");

            var listaPaises = (await RESTCountriesAPI.GetAllCountriesAsync()).Select(c => c.Name).ToList();


            return Ok(listaPaises);
        }

        /// <summary>
        /// Por el nombre parcial o completo de un pais se obtiene la informacíon de el
        /// </summary>
        [HttpGet("buscar")]
        public async Task<IActionResult> Index(string busqueda)
        {

            _logger.LogInformation($"Buscando el pais {busqueda}");

            try
            {
                var pais = (await RESTCountriesAPI.GetCountriesByNameContainsAsync(busqueda)).FirstOrDefault();
                return Ok(pais);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"No se consiguio ningun pais con la palabra{busqueda}, error {e}");
                return NotFound();
            }

        }

        /// <summary>
        /// Este metodo responde una cadena de texto base64 de la bandera flag, donde se ha hecho la conversion del tipo .svg a .png y seguidamente a base64 por un array de bytes
        /// </summary>
        [HttpGet("bandera")]
        public async Task<IActionResult> GetFlag(string country)
        {
            _logger.LogInformation($"Buscando la bandera del pais {country}");

            byte[] bytes;

            try
            {
                var flag = (await RESTCountriesAPI.GetCountriesByNameContainsAsync(country)).Select(x => x.Flag).FirstOrDefault();

                using (WebClient client = new WebClient())
                {
                    client.DownloadFileAsync(new Uri(flag), @"c:\tmp\flag.svg");
                    var svgString = flag;
                    bytes = Encoding.UTF8.GetBytes(flag);
                    var svg = Encoding.UTF8.GetString(bytes);

                }

                return Ok(bytes);

            }
            catch (Exception e)
            {
                _logger.LogWarning($"No se consiguio ninguna bandera, error {e}");
                return NotFound();
            }

        }

        /// <summary>
        /// Este metodo post permite transferir la información de un pais del api al modelo del microservicio para almacenarlo.
        /// </summary>
        /// /// <remarks>
        /// Ejemplo de petición
        ///
        ///     Escribir entre comillas nombre o parte del nombre del pais que se quiere almacenar
        ///
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string country)
        {
            try
            {
                var paisNombre = (await RESTCountriesAPI.GetCountriesByNameContainsAsync(country)).FirstOrDefault();

                Country paisPost = new()
                {
                    Name = paisNombre.Name,
                    Alpha2Code = paisNombre.Alpha2Code,
                    Alpha3Code = paisNombre.Alpha3Code,
                    Capital = paisNombre.Capital,
                    NativeName = paisNombre.NativeName,
                    Region = paisNombre.Region
                };

                _context.Add(paisPost);
                await _context.SaveChangesAsync();

                return Created("", paisPost);
            }
            catch (Exception e)
            {
                _logger.LogWarning($"No se consiguio ninguna pais {country}, error {e}");
                return NotFound();
            }
        }







    }
}
