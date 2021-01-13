# datamock

`datamock` is a tool for initializing your API services with dummy *(or real)* data. It is useful for generating data for rapid prototyping, testing, load testing, ... of your API.

## Installation

`datamock` is implemented as a .NET Core Global Tool, so its installation is quite simple:

```properties
dotnet tool install -g Kros.DummyData.Initializer
```

To update it to the latest version, if it was installed previously, use:

```properties
dotnet tool update -g Kros.DummyData.Initializer
```

## Commands

### run

Run posting mock data to your API.

```properties
datamock run -s D:/dummyData/source
```

#### Parameters

**--source, -s**: Directory where the initialization data is located.

**--verbose, -v**: Verbose.

### preview

Run generating preview data.

```properties
datamock preview -s D:/dummyData -d D:/dummyData/output
```

#### Parameters

**--source, -s**: Directory where the initialization data is located.

**--dest, -d**: Directory where the generated data will be saved.

**--verbose, -v**: Verbose.

**--compress, -c**: Compress final `JSON` written to file.

## Main idea

The purpose of this tool is to generate test data based on templates and send this data to your API. The tool is general, it does everything based on data in a specific directory structure.

### Configuration

The `--source` parameter defines the path to the directory where the data that will be sent to your API is located. The `settings.json` file must be in this directory. Which contains settings for the whole process.

```json
{
    "BaseUrl": "http://localhost:3000",
    "Variables": {
        "variable1": "value1",
        "variable2": "value2"
    },
    "DefaultHeaders": {
        "x-header-1": "header1"
    },
    "Proxy": {
        "Address": "http://192.155.1.1:1111",
        "BypassProxyOnLocal": true
    },
    "Retrying": [
        1,
        3,
        5
    ],
    "RequestTimeOut": 180,
    "AuthOptions": {
        "AuthServer": "https://identityserver.com",
        "ClientId": "yourClient",
        "User":{
            "Name": "userName",
            "Password": "userPassword"
        }
    }
}
```

`BaseUrl`: Base URL to your API. *(It can be overwritten at the level of each request)*

`Variables`: Variables *(key, value)* to be used when processing templates. More below. *(optional)*

`DefaultHeaders`: Default headers to be added to each request.

`Proxy`: Proxy settings for outgoing requests. *(optional)*

`Retrying`: The sleep durations to wait for on each retry. *(optional)*

`RequestTimeOut`: Request timeout in seconds. *(optional)*

`AuthOptions`: Settings for the authorization handler. Currently only IdentityServer is supported. *(optional)*

### Definition of requests

The definition of requests and data that will be sent to your API is in separate directories. Directories are processed in alphabetical order, so I recommend using a numeric prefix to define the order.

```
.
├─ 🗄️ settings.json
├─ 📂001-catalog
│   ├─ 🗄️ request.json
│   └─ 🗄️ catalogs.json
├─ 📂002-users
│   ├─ 🗄️ request.json
│   └─ 🗄️ users.json
├─ 📂003-orders-user1
│   ├─ 🗄️ request.json
│   └─ 🗄️ orders.json
└─ 📂004-orders-user2
    ├─ 🗄️ request.json
    └─ 🗄️ orders.json
```

Each directory must contain a `reqest.json` file that contains the request definition and at least one `JSON` data file that contains the data *(or data template)*.

```json
{
    "BaseUrl": "http://localhost:5000",
    "Name": "catalog",
    "Path": "/api/catalog/",
    "QueryParams": {},
    "Headers": {
        "x-header-1": "header1"
    },
    "Variables": {
        "variable1": "value1"
    },
    "ContinueOnError": true,
    "User":{
        "Name": "userName",
        "Password": "userPassword"
    },
    "ExtractResponseProperty": "id"
}
```

`BaseUrl`: Base URL to your API. *(If not specified, the value from `settings.json` is used)*

`Name`: Name of request. *(Will be used when defining the dependency, see below)*

`Path`: API path.

`QueryParams`: Query parameters (key, value). *(optional)*

`Headers`: Headers to be added to the request. If the same header is in DefaultHeaders, it is overwritten. *(optional)*

`Variables`: Variables *(key, value)* to be used when processing templates. More below. *(optional)*

`ContinueOnError`: Continue with http request error? *(optional, default is `false`)*

`User`: User credentials. *(optional, if not specified, they will be used from `settings.json`)*

`ExtractResponseProperty`: Specifies which property to extract from the response and use as an output parameter. This can then be used in another request. More in the section on addictions. *(optional)*

### Data file

The `JSON` file contains a list of `[]` data that will be sent sequentially to your API using the `POST` method `Content-Type:application/json`. The data can be real or it can be defined as a template. `datamock` uses a great [scriban](https://github.com/lunet-io/scriban) project to preprocess data. This means that you can take advantage of all the [language constructs](https://github.com/lunet-io/scriban/blob/master/doc/language.md) and [builtins functions](https://github.com/lunet-io/scriban/blob/master/doc/builtins.md) that scriban offers. You can also use functions defined directly in this tool and variables that you have defined in the `Variables` section. You will use these variables as follows `{{variables.variable1}}`

Example:

```json
[
    {{ for i in 1..1000 }} 
    {
        "requestId": "{{i}}",
        "code": "10-00-{{i}}",
        "name": "{{lorem_ipsum 30}}",
        "description": "{{lorem_ipsum 60}}",
        "price": {{random_int 10 300}},
        "type": "{{variables.variable1}}"
    }
    {{
        if i < 1000
            ","
        end
    }}
    {{ end }}
]
```

`datamock` uses scriban to generate data based on this template.

The `requestId` property is used to define the dependency between requests. More see the section on dependencies.

### Dependencies

If you want to initialize a complex system, you probably need to resolve the dependencies between individual requests. For example, you create users then you need the `id`s of the created users to properly initialize their orders in the next requests.

In the request definition it is possible to define the `ExtractResponseProperty` property, which describes the response property to be extract from the response and added to the `outputs` variables, which you can use in other requests. The `outputs` variables will be available under the key, which is compose as follows `requestName_requestId`. You can access it as follows `{{outputs.requestName_requestId}}`. In our example, `{{outputs.users_1}}`

`003-orders-user1/request.json`

```json
{
    "Name": "orders_user1",
    "Path": "/api/orders/{{outputs.users_1}}/"
}
```

### Template functions

All from scriban. And a few directly from this tool

#### random_int

Random `int` value. Params: `min` `max`

```properties
random_int 1 55
```

#### random_double

Random `double` value. Params: `min` `max`

```properties
random_double 1 55
```

#### random_first_name

Random person first name.

```properties
random_first_name
```

#### random_last_name

Random person last name.

```properties
random_last_name
```

#### random_email

Random person email.

```properties
random_email
```

#### random_person_name

Random person name. Params: `nameSeparator`.

```properties
random_person_name '-'
```

#### random_person

Random person.

```properties
person = random_person

person.first_name
person.last_name
person.mail
```

#### lorem_ipsum

Random lorem ipsum text. Params: `maxLength`.

```properties
lorem_ipsum 30
```

#### get_by_key

Get value from dictionary by key. 

Params: 

- `dictionary`: dictionary `<string,string>` from which I want the value.
- `key`: the key by which I want the value.

```properties
value = get_by_key outputs "key"
```

#### pad_left

Returns a new string that right-aligns the characters in this instance by padding
them on the left with a specified Unicode character, for a specified total length.

Params: 

- `totalWidth`: the number of characters in the resulting string, equal to the number of original characters plus any additional padding characters.
- `paddingChar`: A Unicode padding character. *(opional, default value is `0`)*

```properties
value | pad_left 2 '0'
```

## Repeat definitions

For `preview` command is allowed function named repating. Sometimes you need to use a same request definition multiple times, for example for multiple users, tenants, ... In this case, you can create a `repeat.json` file in the directory where the request definition is located. This file contains a list of iterations defined by name, and it is possible to add custom variables to each iteration to be used in further processing.

```json
[
 {
     "Name": "company1",
     "Variables":{
         "userName": "user1@gmail.com",
         "userPassword": "password"
     }
 },
 {
    "Name": "company2",
    "Variables":{
        "userName": "user2@gmail.com",
        "userPassword": "password"
    }
 },
 {
    "Name": "company3",
    "Variables":{
        "userName": "user3@gmail.com",
        "userPassword": "password"
    }
 }
]
```

> This file is also processed using `scriban`, so it is possible to use its features.

A separate subdirectory is created for each iteration according to the name of the iteration. The definition of the request and its data will be modified in the given subdirectory.

> Variables from repeat definition are add to `output` variables by key `name_variableKey`. E.g.: `{{outputs.company1_userName}}`.

### Dependencies

If you need to reference to the name of the current iteration, it is available in the variable `{{variable.index}}`. This is needed, for example, in the `requestId` definition.

```json
{
    "requestId": "{{variables.index}}_{{i}}"    
},
```

In other requests, where we use `repeats`, and in each iteration we want to refer outputs from different iteration of other requests, we must do it indirectly through variables. In the definition of iteration we define the required variable, for example `companyId`.

```json
{
    "Name": "company1-user1",
    "Variables":{
        "companyId": "{{ outputs.companies_company1_1 }}",
        "userName": "user1@gmail.com",
        "userPassword": "password"
    }
},
```

Next, we can use this variable in the request definition.

```json
{
    "Name": "invoices",
    "Path": "/companies/{{variables.companyId}}/invoices",
    "User":{
        "Name": "{{variables.userName}}",
        "Password": "{{variables.userPassword}}"
    }
}
```
