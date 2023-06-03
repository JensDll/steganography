terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.58.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "2.39.0"
    }
  }

  required_version = ">= 1.4.0"
}

provider "azurerm" {
  features {}
}

data "azurerm_subscription" "primary" {}

data "azuread_client_config" "current" {}

resource "azurerm_resource_group" "resources" {
  name     = "steganography_resources"
  location = "francecentral"
}
