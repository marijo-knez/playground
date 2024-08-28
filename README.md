Principles:
- Hexagonal architecture
- DDD
- Modular monolith with bounded context equals module
- CQS - use separate backend models for query (reads) vs command (data changes) but use the same db

Project/folder structure:
- Folder for each module named [ProductName].[ModuleName]
- [ProductName].Domain.[ModuleName] project for each module (bounded context)
- under it we add folders by feature names
- here goes the aggeregate root class, entities, events, value objects, domain services, related to the feature and its aggregate roots
- avoid adding shared folders, but it is allowed (if we need to add to shared, it should make us think if our aggregates are well formed, and also do not overforce the DRY principle)
- persistance folder for repository interfaces
- internal contracts for interfaces that communicate with other modules
- external contracts for interfaces that communicate with external systems
- [ProductName].Domain.Base project for code common across all bounded contextes
- IAggregateRoot
- IEntity
- IValueObject
- ...
- [ProductName].Persistance.[ModuleName] project
- db scheme name = ModuleName
- has tables only form it's db scheme
- class for each table
- ef configuration mapping for class to table
- can have views on tables form other modules (think dbcontext has issues if two models import same tables), so it will disable updates on different schema tables, and will be a clear indicator where integration is happening
- has folder repositories with implementation of it's modules repositories
- maps class of each table to domain object property
- can use mappers but maybe no need... don't know how much reusage is there really since this is only for command part
- has implementation if IUnitOfWork for it's module
- Migrations folder for migration scripts
- [ProductName].Application.Command.[ModuleName] project
- under it we add folders by feature name
- here goes the applicationService, commands, command results
- IUnitOfWork
- security - classes with queries that check access
- [ProductName].Application.Query.[ModuleName] project
- under it we add [QueryDescription]Query.cs class for each query
- [QueryDescription]QueryInput.cs - query input class
- [ViewDescrittion]View.cs - query resutls class
- does not use the Domain projects, but uses the Persistance of the module to write queries
- security - classes with queries that check access - try to write them as queriables that can be part of the content query for better performance
- [ProductName].API.[ModuleName]
- under feature name we add controllers with either Query or Command sufix that implement api endpoints and call application.Command or application.Query
- reuse queryInputs, Views, commands and commandresults from applicationService, don't map to new objects... if needed (eg. these objects can't serialize, than do it on a case by case basis)
- [ProductName].Internal.[ModuleName]
- implementation of internal contracts from other modules that depend on this module
- this is to avoid polluting the domain with other modules needs, and have a clear sense of who depends on this module
- [ProductName].External.[ModuleName]
- implementation of interfaces used by this module that communicate to external systems
- [ProductName].AsyncJobs.[ModuleName] (optional)
- use to start hangfire and execute async jobs

DDD:
- Try to use small aggregates, better to use two if possible, than one large. Think how many properties of the aggergate does your command really need, if it's just a few, and the aggergate is already heavy, consider creating a new one. Usualy if one command changes one property, the other just read's it, so we can split them, since we won't violate invariant. Of course we prefer to have all actions on the same object, if it's performant :).
- Use access modifiers to expres the intent to others using your code what you meant. Only aggergates and only it's application used methods are set as public, only methods used by other objects are internal, and everything else is private (or protected).
- All of our aggregates and entities need public constructors so we can create them in our repositories, these are not to be used anywhere else
- Try to name your aggregates and methods to reflect behaviour, not structure or crud. Eg. updateState to Approve, Deny.
- favor more api's with simpler logic than fewer with complex logic. Eg. instead of sending all 10 properties to update, with one property having side effects (changing somehting else based on it's value), try to brake it into two, with the one property having side effects clearly separated on the ui (since it will most likely be more intuitive to the user that somehtin other than just field update is going on) and having a clear behaviour drive name with it :)
- It is fine to have same name classes, even behaviour in different modules. Thing is that once we decided a boundary of a bounded countext, we did say that changes to it will happen only for reasons inside it, so we don't want to reuse stuff from other modules, even if it behaves the same, because it will probably have a different reason to change. If we do see a lot of same behavoiur going on, maybe we should revisit our bounded context boundaries.
- use ResultPattern rather than exceptions for envorcine invariants and validations
- validate command in app layer with stuff it can validate (types, lenght, not null) to improve performance (not having to load a heavy aggeregate just to check for null)
- validate aggregate inside aggregate for stuff the aggregate knows (it's own properties)
- validate in domain service stuff that is outside of the aggregate
- security checks and transaction commits are done by Aplication layer
- event handlers
- use inside bounded countext to enforce changes in same transaction (aggregate to aggregate change)
- use inside internal to add to async jobs (we should try to use jobs, since they can be replayed, not calling another change in a different transaction that if fails is gonna lead to inconsistency)
- use interfaces to force same transacaction between bounded contexts
- use inside external to add async jobs for external systems (email, payment service...)
- avoid using dtc if possible :)
- avoid passing domain services into aggergates, since it add a outside dependency to them... rather force the change on the aggregate by making the method on aggregate internal, and forcing the usege of the domain service which changes the aggregate
- try to use value objects for properties that change togeather (eg. price which has amount and currency, use a separate class instead of having amount and currency on entity)



Querstions:
- IUnitOfWork goes into app layer, because it's the only one that is gonna use it, it's controlling transactions... think we don't need it in domain, right?
- transaction accross modules with different UnitsOfWork on each module wrapped in transaction?
- feature name is the application service (without the service part) name also maybe equals to aggreget root? Strange to have the feature name in domain without it having a coresponding object name, but maybe best to have it only like a namspace to group many objects that implement that high level feature
- in application layer we use application services... we could use the command handlers instead, and mediator so the caller does not need to know which app service to call, it can just send the command... but seems to add complexity for no reason, or does it?
- do we need an Internal project, and should it have implementation of others modules interfaces by using it's domain, or should it have implementations of it's own interfaces by using other modules domains?
- do we want to use identity classes?
- class for each table and mapping in repository vs using domain objects directly in configuration mapping? Went with first for now.
- use events or explicitly in application service when changing two aggregates form same or different context in same transaction?
- use domain events for cross bounded context communication? how? how is it different from using interfaces? hangfire fro eventual consistency scenarios? event handlers outside of transaction through async jobs only?
- example code of what goes to app service vs what goes to domain service
- aggregate story - each method (or a small number of them) in it's own interface, than with implementation on class, instead of adding a method directly on a class, so we can end up with one class with many interface implementations. Than we only use the interface where we need, and could potentionally have a repository for each interface to just fill the part of the aggregate needed for that interface. Needs adidtional research, but sounds like we would still have a partially loadaded aggregate and enforcing invariants would be questionable right?
