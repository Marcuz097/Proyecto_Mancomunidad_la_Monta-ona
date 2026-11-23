namespace LaMontanona.Components.Pages;

public static class Contenido
{
    public record Programa(string Icono, string Titulo, string Resumen, string Detalle, string Imagen, string Posicion, string[] Lineas);

    public static readonly Programa[] Programas =
    {
        new("M12 21V8M12 8c-4 0-6-3-6-6 4 0 6 2 6 6zm0 4c4 0 6-3 6-6-4 0-6 2-6 6z", "Conservación forestal", "Protección del bosque, reforestación y manejo responsable del territorio.", "Protección del bosque, reforestación y manejo responsable del territorio para conservar la biodiversidad.", "/img/gigante-natural.jpg", "center", ["Protección del bosque", "Reforestación", "Manejo responsable del territorio"]),
        new("M12 3s6 7 6 11a6 6 0 0 1-12 0c0-4 6-11 6-11z", "Gestión del agua", "Cuidado de fuentes, captación y acuerdos comunitarios para asegurar el recurso hídrico.", "Cuidado de fuentes, captación y acuerdos comunitarios para asegurar el acceso sostenible al recurso hídrico.", "/img/agua-el-salvador.jpg", "center", ["Cuidado de fuentes", "Captación de agua", "Acuerdos comunitarios"]),
        new("M3 12c3-4 6-4 9 0s6 4 9 0M3 17c3-4 6-4 9 0s6 4 9 0", "Producción sostenible", "Sistemas agropastoriles y alternativas económicas compatibles con la conservación.", "Promoción de sistemas agropastoriles y alternativas económicas compatibles con la conservación del territorio.", "/img/fuente-vida.jpg", "center", ["Sistemas agropastoriles", "Alternativas económicas", "Producción compatible con la conservación"]),
        new("M16 20v-2a4 4 0 0 0-4-4H7a4 4 0 0 0-4 4v2M9.5 10a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7zM17 10l2 2 3-3", "Participación comunitaria", "Organización, formación y liderazgo local para la gobernanza del territorio.", "Fortalecimiento de la organización, la formación y el liderazgo local para mejorar la gobernanza del territorio.", "/img/SRLAguayGobernanza.jpg", "center", ["Organización comunitaria", "Formación y liderazgo", "Gobernanza del territorio"]),
        new("M4 20h16M6 20V8l6-4 6 4v12M9 12h6M9 16h6", "Turismo y desarrollo local", "Senderos, conocimientos locales y emprendimientos que generan beneficios compartidos.", "Desarrollo de senderos, valoración de conocimientos locales y emprendimientos que generan beneficios compartidos.", "/img/turismo-sendero.jpg", "center", ["Senderos locales", "Conocimientos locales", "Emprendimientos compartidos"])
    };
}
