Descripción
TinyJSON es una biblioteca JSON simple para C # que busca la facilidad de uso.

Características
Transfigura objetos en JSON y viceversa.
Utiliza la reflexión para volcar y cargar gráficos de objetos automáticamente.
Admite primitivas, clases, estructuras, enumeraciones, listas, diccionarios y matrices.
Admite matrices unidimensionales, matrices multidimensionales y matrices dentadas.
Los datos analizados utilizan variantes de proxy que se pueden convertir implícitamente en tipos primitivos para un código más limpio o codificar directamente en JSON.
Los tipos numéricos se manejan sin problemas.
Clases polimórficas compatibles con una sugerencia de tipo codificada en JSON.
Admite salida JSON de impresión bonita opcional.
Admite, opcionalmente, codificar propiedades y campos privados.
Admite la decodificación de campos y propiedades a partir de nombres con alias.
Unidad probada.
Uso
La API tiene un espacio de nombres debajo TinyJSONy la clase principal es JSON. En realidad, solo hay algunos métodos que necesita saber:

espacio de nombres  TinyJSON
{
	 JSON de clase estática  pública 
	{
		 carga de variante estática  pública ( cadena json );
		Volcado público de cadenas estáticas ( datos de objeto , EncodeOptions = EncodeOptions . Ninguno );
		public static void MakeInto < T > ( Variant data , out T item );             
	}
}
Load()cargará una cadena de JSON, devuelve nullsi no es válido o un Variantobjeto proxy si tiene éxito. El proxy permite conversiones implícitas y puede convertir entre varios tipos de valores numéricos de C #.

var  datos  =  JSON . Load ( " { \" foo \ " : 1, \" bar \ " : 2.34} " );
int  i  =  datos [ " foo " ];
float  f  =  datos [ " barra " ];
Dump() tomará un objeto, lista, diccionario o tipo de valor primitivo de C # y lo convertirá en JSON.

var  data  =  new  List < int > () {{ 0 }, { 1 }, { 2 }};
Consola . WriteLine ( JSON . Dump ( datos )); // salida: [1,2,3]
TinyJSON también puede manejar clases, estructuras, enumeraciones y objetos anidados. Dadas estas definiciones:

enumeración  TestEnum
{
	Thing1 ,
	 Thing2 ,
	 Thing3
}


struct  TestStruct
{
	public  int  x ;
	public  int  y ;
}


clase  TestClass
{
	 nombre de cadena  pública ;
	tipo TestEnum público ;
	Lista pública < TestStruct > datos = nueva Lista < TestStruct > ();      

	[ Excluir ]
	 public  int  _ignored ;

	[ BeforeEncode ]
	 public  void  BeforeEncode ()
	{
		Consola . WriteLine ( "¡ Devolución de llamada BeforeEncode activada! " );
	}

	[ AfterDecode ]
	 public  void  AfterDecode ()
	{
		Consola . WriteLine ( "¡ Devolución de llamada AfterDecode activada! " );
	}
}
El siguiente código:

var  testClass  =  new  TestClass ();
testClass . nombre  =  " Rumpelstiltskin Jones " ;
testClass . tipo  =  TestEnum . Thing2 ;
testClass . datos . Agregar ( new  TestStruct () { x  =  1 , y  =  2 });
testClass . datos . Agregue ( new  TestStruct () { x  =  3, y  =  4 });
testClass . datos . Agregar ( new  TestStruct () { x  =  5 , y  =  6 });

var  testClassJson  =  JSON . Dump ( testClass , verdadero );
Consola . WriteLine ( testClassJson );
Saldrá:

{
	 " name " : " Rumpelstiltskin Jones " ,
	 " type " : " Thing2 " ,
	 " data " : [
		{
			" x " : 1 ,
			 " y " : 2
		},
		{
			" x " : 3 ,
			 " y " : 4
		},
		{
			" x " : 5 ,
			 " y " : 6
		}
	]
}
Puede usar, MakeInto()se puede usar para reconstruir datos JSON nuevamente en un objeto:

TestClass  testClass ;
JSON . MakeInto ( JSON . Load ( testClassJson ), fuera  testClass );
También hay Make()métodos Variantque proporcionan opciones para una sintaxis un poco más natural:

TestClass  testClass ;

JSON . Carga ( json ). Make ( out  testClass );
// o 
testClass  =  JSON . Carga ( json ). Hacer < Datos > ();
Finalmente, notará que TestClasstiene los métodos BeforeEncode()y los AfterDecode()que tienen los atributos TinyJSON.BeforeEncodey TinyJSON.AfterDecode. Estos métodos se llamarán antes de que el objeto comience a serializarse y después de que el objeto se haya deserializado por completo. Esto es útil cuando se requiere alguna preparación adicional o lógica de inicialización.

De forma predeterminada, solo se codifican los campos públicos, no las propiedades ni los campos privados. Puede etiquetar cualquier campo o propiedad para que se incluya con el TinyJSON.Includeatributo, o forzar la exclusión de un campo público con el TinyJSON.Excludeatributo.

Decodificar alias
Los campos y las propiedades se pueden decodificar a partir de alias mediante el TinyJSON.DecodeAliasatributo. Durante la decodificación, si no se encuentran datos coincidentes en el JSON para un campo o propiedad determinados, también se buscarán sus alias.

clase  TestClass
{
	[ DecodeAlias ( " otroNombre " )]
	 nombre de cadena pública  ; // decodificar desde "nombre" u "otroNombre" 

	[ DecodeAlias ( " anotherNumber " , " yetAnotherNumber " )]
	 public  int  number ; // decodificar desde "número", "otroNúmero" o "aúnOtroNúmero" 
}
Tipo de sugerencia
Al decodificar tipos polimórficos, TinyJSON no tiene forma de saber qué subclase instanciar a menos que se incluya una sugerencia de tipo. Entonces, de forma predeterminada, TinyJSON agregará una clave nombrada @typea cada objeto codificado con el tipo de objeto completamente calificado.

Opciones de codificación
Actualmente, hay varias opciones disponibles para la codificación JSON y se pueden pasar como un segundo parámetro a JSON.Dump().

EncodeOptions.PrettyPrint generará JSON con un formato agradable para que sea más legible.
EncodeOptions.NoTypeHintsdeshabilitará la salida de sugerencias de tipo en la salida JSON. Esto puede ser conveniente si planea leer el JSON en otra aplicación que podría ahogarse con la información del tipo. Puede anular esto por miembro con el TinyJSON.TypeHintatributo.
EncodeOptions.IncludePublicProperties incluirá propiedades públicas en la salida.
EncodeOptions.EnforceHeirarchyOrder asegurará que los campos y las propiedades estén codificados en orden de jerarquía de clases, desde la clase base raíz hacia abajo, pero tiene un pequeño costo de rendimiento.
Usando variantes
Para la mayoría de los casos de uso, puede simplemente asignar, emitir o hacer su gráfico de objetos utilizando la API descrita anteriormente, pero a veces es posible que deba trabajar con los objetos de proxy intermedios para, digamos, explorar e iterar sobre una colección. Para hacer esto, envíe el Varianta la subclase apropiada (probablemente ProxyArrayo ProxyObject) y estará listo:

var  list  =  JSON . Cargar ( " [1,2,3] " );
foreach ( elemento var  en la lista como ProxyArray )    
{
	int  número  =  artículo ;
	Consola . WriteLine ( número );
}

var  dict  =  JSON . Cargar ( " { \" x \ " : 1, \" y \ " : 2} " );
foreach ( par var  en dict como ProxyObject )    
{
	 valor  flotante =  par . Valor ;
	Consola . WriteLine ( par . Clave  +  " = "  +  valor );
}
Esta no percepción Variantsubclases son ProxyBoolean, ProxyNumbery ProxyString. También puede ser una variante null.

Cualquier Variantobjeto se puede codificar directamente en JSON llamando a su ToJSON()método o pasándolo a JSON.Dump().

Notas
Este proyecto se desarrolló teniendo en cuenta la eliminación del dolor y el tamaño ligero. Debería poder manejar cantidades razonables de datos razonables a velocidades razonables, pero no está diseñado para conjuntos de datos masivos.

El caso de uso principal de esta biblioteca es con Unity3D, por lo que la compatibilidad se centra allí, aunque debería funcionar con la mayoría de los entornos C # modernos.

Se ha utilizado en varios juegos publicados. Es bueno para preferencias, datos de nivel y progreso, etc.

Meta
Hecho a mano por Patrick Hogan [ twitter • github • sitio web ]

Basado en MiniJSON de Calvin Rien

Publicado bajo la licencia MIT .
