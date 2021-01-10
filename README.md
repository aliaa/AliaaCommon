# AliaaCommon  
This project contains every common codes that I use in my C# projects.  
It contains 4 projects:
- __AliaaCommon__: A set of utility classes for string checking mechanisms, Persian Calendar conversions, Display Utils, HSL Color, etc...  
- __AliaaCommon.Blazor__: A set of some components and codes that I use in common in my blazor projects:
    - Components:
        - CheckBoxList: A component to show checkboxes in a list with some styling settings.
        - Label: Show label of a member of a type (using reflection).
        - Loading: Bootstrap loading wrapped as component.
        - ObjectTable: A comprehensive component to show a table of objects (using reflection) with options like sticky head, fields include/exclude, action buttons, column background colors,...
    - Utils:
        - HttpClientX: A rewritten version of System.Net.Http.HttpClient in order to easily catch and retrieve errors, send advanced POST request with generic both request and response types and also download and upload files.  
        
- __AliaaCommon.Models__: Some common models like AuthUser
- __AliaaCommon.Server__: Some server side utilities like Persian characters normalization, tasks piplining, ...
