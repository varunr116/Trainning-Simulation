mergeInto(LibraryManager.library, {
  SCORM_Initialize: function () {
    try {
      console.log("[SCORM Bridge] Attempting to initialize...")

      // Check for SCORM 1.2 API
      if (window.parent && window.parent.API) {
        var api = window.parent.API
        var result = api.LMSInitialize("")
        console.log("[SCORM Bridge] SCORM 1.2 Initialize:", result)
        return result === "true"
      }

      // Check for SCORM 2004 API
      if (window.parent && window.parent.API_1484_11) {
        var api = window.parent.API_1484_11
        var result = api.Initialize("")
        console.log("[SCORM Bridge] SCORM 2004 Initialize:", result)
        return result === "true"
      }

      // Check for API in current window
      if (window.API) {
        var api = window.API
        var result = api.LMSInitialize("")
        console.log("[SCORM Bridge] Direct API Initialize:", result)
        return result === "true"
      }

      if (window.API_1484_11) {
        var api = window.API_1484_11
        var result = api.Initialize("")
        console.log("[SCORM Bridge] Direct API 2004 Initialize:", result)
        return result === "true"
      }

      console.warn("[SCORM Bridge] No SCORM API found - running in test mode")
      return true // Allow testing without LMS
    } catch (e) {
      console.error("[SCORM Bridge] Initialize error:", e)
      return false
    }
  },

  SCORM_SetValue: function (element, value) {
    try {
      var elementStr = UTF8ToString(element)
      var valueStr = UTF8ToString(value)

      console.log("[SCORM Bridge] SetValue:", elementStr, "=", valueStr)

      // Try SCORM 1.2 first
      if (window.parent && window.parent.API) {
        var api = window.parent.API
        var result = api.LMSSetValue(elementStr, valueStr)
        return result === "true"
      }

      // Try SCORM 2004
      if (window.parent && window.parent.API_1484_11) {
        var api = window.parent.API_1484_11
        var result = api.SetValue(elementStr, valueStr)
        return result === "true"
      }

      // Try direct API
      if (window.API) {
        var api = window.API
        var result = api.LMSSetValue(elementStr, valueStr)
        return result === "true"
      }

      if (window.API_1484_11) {
        var api = window.API_1484_11
        var result = api.SetValue(elementStr, valueStr)
        return result === "true"
      }

      // Test mode - just log
      console.log("[SCORM Bridge] Test mode - SetValue logged")
      return true
    } catch (e) {
      console.error("[SCORM Bridge] SetValue error:", e)
      return false
    }
  },

  SCORM_GetValue: function (element) {
    try {
      var elementStr = UTF8ToString(element)
      var result = ""

      // Try SCORM 1.2 first
      if (window.parent && window.parent.API) {
        var api = window.parent.API
        result = api.LMSGetValue(elementStr)
      }
      // Try SCORM 2004
      else if (window.parent && window.parent.API_1484_11) {
        var api = window.parent.API_1484_11
        result = api.GetValue(elementStr)
      }
      // Try direct API
      else if (window.API) {
        var api = window.API
        result = api.LMSGetValue(elementStr)
      } else if (window.API_1484_11) {
        var api = window.API_1484_11
        result = api.GetValue(elementStr)
      } else {
        // Test mode - return sample data
        switch (elementStr) {
          case "cmi.core.student_name":
            result = "Test Student"
            break
          case "cmi.core.lesson_status":
            result = "incomplete"
            break
          default:
            result = ""
        }
      }

      console.log("[SCORM Bridge] GetValue:", elementStr, "=", result)

      var bufferSize = lengthBytesUTF8(result) + 1
      var buffer = _malloc(bufferSize)
      stringToUTF8(result, buffer, bufferSize)
      return buffer
    } catch (e) {
      console.error("[SCORM Bridge] GetValue error:", e)
      var emptyStr = ""
      var bufferSize = lengthBytesUTF8(emptyStr) + 1
      var buffer = _malloc(bufferSize)
      stringToUTF8(emptyStr, buffer, bufferSize)
      return buffer
    }
  },

  SCORM_Commit: function () {
    try {
      // Try SCORM 1.2 first
      if (window.parent && window.parent.API) {
        var api = window.parent.API
        var result = api.LMSCommit("")
        console.log("[SCORM Bridge] Commit result:", result)
        return result === "true"
      }

      // Try SCORM 2004
      if (window.parent && window.parent.API_1484_11) {
        var api = window.parent.API_1484_11
        var result = api.Commit("")
        console.log("[SCORM Bridge] Commit result:", result)
        return result === "true"
      }

      // Try direct API
      if (window.API) {
        var api = window.API
        var result = api.LMSCommit("")
        console.log("[SCORM Bridge] Commit result:", result)
        return result === "true"
      }

      if (window.API_1484_11) {
        var api = window.API_1484_11
        var result = api.Commit("")
        console.log("[SCORM Bridge] Commit result:", result)
        return result === "true"
      }

      console.log("[SCORM Bridge] Test mode - Commit logged")
      return true
    } catch (e) {
      console.error("[SCORM Bridge] Commit error:", e)
      return false
    }
  },

  SCORM_Terminate: function () {
    try {
      // Try SCORM 1.2 first
      if (window.parent && window.parent.API) {
        var api = window.parent.API
        var result = api.LMSFinish("")
        console.log("[SCORM Bridge] Terminate result:", result)
        return result === "true"
      }

      // Try SCORM 2004
      if (window.parent && window.parent.API_1484_11) {
        var api = window.parent.API_1484_11
        var result = api.Terminate("")
        console.log("[SCORM Bridge] Terminate result:", result)
        return result === "true"
      }

      // Try direct API
      if (window.API) {
        var api = window.API
        var result = api.LMSFinish("")
        console.log("[SCORM Bridge] Terminate result:", result)
        return result === "true"
      }

      if (window.API_1484_11) {
        var api = window.API_1484_11
        var result = api.Terminate("")
        console.log("[SCORM Bridge] Terminate result:", result)
        return result === "true"
      }

      console.log("[SCORM Bridge] Test mode - Terminate logged")
      return true
    } catch (e) {
      console.error("[SCORM Bridge] Terminate error:", e)
      return false
    }
  },
})
