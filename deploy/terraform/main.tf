terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.58.0"
    }
  }

  required_version = ">= 1.4.0"
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "resources" {
  name     = "steganography_resources"
  location = "francecentral"
}

# resource "azurerm_service_plan" "service_plan" {
#   name                = "steg_service_plan"
#   resource_group_name = azurerm_resource_group.rg.name
#   location            = azurerm_resource_group.rg.location
#   os_type             = "Linux"
#   sku_name            = "B1"
# }

module "active_directory" {
  source = "./active_directory"
}
