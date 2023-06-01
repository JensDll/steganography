terraform {
  required_providers {
    azuread = {
      source  = "hashicorp/azuread"
      version = "2.39.0"
    }
  }

  required_version = ">= 1.4.0"
}

data "azuread_client_config" "current" {}

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
  subject               = "repo:jensdll/steganography:environment:Azure"
}
