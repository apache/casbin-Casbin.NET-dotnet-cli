// Copyright 2025 The casbin Authors. All Rights Reserved.  
//  
// Licensed under the Apache License, Version 2.0 (the "License");  
// you may not use this file except in compliance with the License.  
// You may obtain a copy of the License at  
//  
//      http://www.apache.org/licenses/LICENSE-2.0  
//  
// Unless required by applicable law or agreed to in writing, software  
// distributed under the License is distributed on an "AS IS" BASIS,  
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  
// See the License for the specific language governing permissions and  
// limitations under the License.

using CasbinCli.Models;  
using FluentAssertions;  
using Newtonsoft.Json;  
using Xunit;  
  
namespace CasbinCli.Tests.Models  
{  
    public class ResponseBodyTests  
    {  
        [Fact]  
        public void ResponseBody_SerializesToJson_CorrectFormat()  
        {  
            // Arrange  
            var response = new ResponseBody  
            {  
                Allow = true,  
                Explain = new[] { "Policy matched" }  
            };  
  
            // Act  
            var json = JsonConvert.SerializeObject(response);  
  
            // Assert  
            json.Should().Contain("\"allow\":true");  
            json.Should().Contain("\"explain\":[\"Policy matched\"]");  
        }  
  
        [Fact]  
        public void ResponseBody_EmptyExplain_SerializesCorrectly()  
        {  
            // Arrange  
            var response = new ResponseBody  
            {  
                Allow = false,  
                Explain = Array.Empty<string>()  
            };  
  
            // Act  
            var json = JsonConvert.SerializeObject(response);  
  
            // Assert  
            json.Should().Contain("\"allow\":false");  
            json.Should().Contain("\"explain\":[]");  
        }  
    }  
}