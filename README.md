# Deployment and Setup

### Prerequisites
- Install the latest version of [Git](https://git-scm.com/downloads)
- Install [.NET SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) v8.0
- Install the [.NET Core Hosting Bundle](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (if hosting on IIS)
- IIS Enabled (if hosting on IIS)

### Steps
1) Clone the repository.
2) Open a CLI of your choice, and navigate to the IVFSuccessCalculator project folder. *(ie - if you checked out to C:\Code, navigate to C:\Code\IVFSuccessCalculator\IVFSuccessCalculator)*
3) Start the application by invoking **dotnet run --launch-profile "https"**

Alternatively, if you wish to deploy to your local IIS, several additional steps must be taken:

1) Publish the application to a directory of your choice, by invoking the following command: **dotnet publish -c Release --output {TARGET DIRECTORY}**
2) Create a new website in IIS, assigning the physical path to the publish directory from the prior step.
3) Start the website.

# Usage

### API

To calculate the IVF Success rate, issue a **POST** request to the **/success-rate** endpoint on the server.
The body of the request must contain the calculation parameters, formatted as a JSON structure.

The skeleton of this request payload is depicted below:
```javascript
{
  age: number, // years
  weight: number, // lbs
  height: number, // inches (ie - 5ft 8) = 68"

  usingOwnEggs: boolean,
  usedIvfBefore: boolean,

  reasonForInfertilityKnown: boolean,
  infertilityDiagnosis: {
      tubalFactor: boolean,
      maleFactorInf: boolean,
      endometriosis: boolean,
      ovulatoryDisorder: boolean,
      diminishedOvarianReserve: boolean,
      uterineFactor: boolean,
      otherReason: boolean,
      unexplainedInf: boolean
  },

  numPriorPregnancies: number,
  numLiveBirths: number
}
```

*Note: All top level request parameters must be included. However, for the infertility diagnosis, selectively specifying infertility factors is allowed.*

As an example, specifying

```javascript
infertilityDiagnosis: {
  tubalFactor: true,
  maleFactorInf: true
}
```

states that only tubal and male infertility are contributing factors. 
The other infertility parameters will be set to false by default.

* * *

#### Response
If the calculation was successful, a 200 level response will be served, containing the success rate probability (expressed as a floating point integer from 0 to 1).


If the request was deemed invalid, a 400 level response will be served instead. 
A validation error payload will be returned in the body.

Example:
```csharp
{
    "Age": [
        "'Age' must be between 20 and 50. You entered 300."
    ],
    "InfertilityDiagnosis": [
        "Infertility factors cannot be specified when reason for infertility is not known."
    ]
}
```

### Website

For convienence, the solution includes a sample webpage that can be used in conjunction with the API. 
Navigate to the Web folder, and open the **success-rate.html** page in a text editor.

Ensure the **API_SERVER_URL** variable is set to the server url configured in the Deployment and Setup section.
Save the page, and open the HTML file in a browser.

A form will be displayed, allowing you to enter in all of the patient details.
When finished, press **Calculate** to issue the request to the server.
After the result is returned, a message will be displayed indicating the success rate (displayed as a percentage).

# Testing

The solution is packaged with unit and integration tests.

To execute, navigate to the *IVFSuccessCalculator.Tests* directory.
Then, invoke the command **dotnet test**.

*Note: For integration tests, the web server must be spun up beforehand.
Ensure the server is up and operational, and that the ServerFixture is pointing to the correct server domain and port (default is localhost:7174).*
