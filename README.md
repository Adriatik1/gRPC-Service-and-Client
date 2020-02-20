# gRPC-Service-and-Client
Krijimi i nje sherbimi gRPC dhe dy klienteve qe konsumojne kete gRPC Sherbim

Ky sistem paraqet mundesine e krijimit te nje API me ane te gRPC Framework-ut ku per strukturim dhe transportim 
te te dhenave perdoren Google Protocol Buffers. Eshte menduar qe ky API te zhvillohet per nje market online
ku jane krijuar dy klient, njeri per stafin dhe tjetri per konsumatoret. Pasi qe me gRPC na u lejohet stream 
bi-directional atehere mundemi qe nga klienti i stafit, te regjistrojme ne vazhdimesi klient dhe ne vazhdimesi
te na u kthehen rezultatet e ketyre regjistrimeve, pra full duplex komunikim. Nga klienti i stafit mund te percillen
edhe blerjet e produkteve ne vazhdimesi ne kohe reale qe behen ne sistem. Po ashtu mundesohet edhe shtimi i produkteve 
ne vazhdimesi.

Kur themi 'ne vazhdimesi' eshte fjala qe nje operacion mund ta bejme periodikisht duke mbajtur vetem nje lidhje online.
Pra, gRPC perdore protokollin HTTP/2, version i cili mundeson qe me nje lidhje klient-server te barten shume e shume te
dhena.

Klienti i ndertuar per konsumator mundeson shfaqjen e produkteve te cilat ofrohen per blerje, blerjen e produkteve ne 
vazhdimesi dhe dergimin e komenteve(feedbacks) ne vazhdimesi.

Perdorimi i Google Protocol Buffers eshte nje perparesi e madhe ne krahasim me gjuhet JSON ose XML, pasi qe ben enkodimin 
e te dhenave ne menyre qe te bartja te behet me lehte dhe me shpejt, pastaj role te madh luan edhe perdorimi i HTTP/2 protokollit.

Sistemi eshte zhvilluar ne C# perkatesisht ne .NET Core 3. Te dhenat jane ruajtur ne SQL Server 18. Si dhe eshte perdorur SQL Dependency
per njoftim te ndryshimeve te te dhenave nga databaza.
