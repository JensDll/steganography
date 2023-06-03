resource "azuread_application" "app" {
  display_name = "steganography"
  owners       = [data.azuread_client_config.current.object_id]
  api {
    requested_access_token_version = 2
  }
}

resource "azuread_application_federated_identity_credential" "app" {
  application_object_id = azuread_application.app.object_id
  display_name          = "steganography_azure"
  description           = "Azure environment on GitHub"
  audiences             = ["api://AzureADTokenExchange"]
  issuer                = "https://token.actions.githubusercontent.com"
  subject               = "repo:JensDll/steganography:environment:Azure"
}

resource "azuread_service_principal" "app" {
  application_id               = azuread_application.app.application_id
  app_role_assignment_required = true
  owners                       = [data.azuread_client_config.current.object_id]
}

resource "azurerm_role_assignment" "example" {
  scope                = data.azurerm_subscription.primary.id
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.app.object_id
}
