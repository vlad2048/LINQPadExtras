namespace LINQPadExtras.PageServing.Replying.Structs;

enum ContentType
{
	String,
	File
}

record ContentNfo(
	ReplyType Type,
	ContentType ContentType,
	string Str
);